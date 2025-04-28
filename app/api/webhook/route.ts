import { Webhook } from 'svix';
import { headers } from 'next/headers';
import { WebhookEvent } from '@clerk/nextjs/server';
import { NextResponse } from 'next/server';
import { employeeQueries } from '@/db/queries';
import { clerkClient } from '@clerk/nextjs/server';

export async function POST(req: Request) {
  // Get the headers
  const headerPayload = await headers();
  const svix_id = headerPayload.get('svix-id');
  const svix_timestamp = headerPayload.get('svix-timestamp');
  const svix_signature = headerPayload.get('svix-signature');

  // If there are no svix headers, this isn't a Clerk webhook
  if (!svix_id || !svix_timestamp || !svix_signature) {
    return new Response('Error: Not a valid Clerk webhook', {
      status: 400
    });
  }

  // Get the body
  const payload = await req.json();
  const body = JSON.stringify(payload);

  // Get Clerk webhook secret from environment variable
  const webhookSecret = process.env.CLERK_WEBHOOK_SECRET;
  
  // Validate webhook secret
  if (!webhookSecret) {
    console.error('Error: CLERK_WEBHOOK_SECRET is not set');
    return new Response('Error: CLERK_WEBHOOK_SECRET is not set', {
      status: 500
    });
  }

  // Create a new Svix instance with your webhook secret
  const webhook = new Webhook(webhookSecret);

  let event: WebhookEvent;
  
  try {
    // Verify the webhook payload
    event = webhook.verify(body, {
      'svix-id': svix_id,
      'svix-timestamp': svix_timestamp,
      'svix-signature': svix_signature,
    }) as WebhookEvent;
  } catch (err) {
    console.error('Error verifying webhook:', err);
    return new Response('Error verifying webhook', {
      status: 400
    });
  }

  // Handle the webhook
  const eventType = event.type;
  
  if (eventType === 'user.created') {
    // Get the user data from the event
    const { id, email_addresses, first_name, last_name } = event.data;
    const email = email_addresses[0]?.email_address;
    
    if (!email) {
      return new Response('Error: No email address found', { status: 400 });
    }
    
    try {
      // Check if there are any existing employees
      const employees = await employeeQueries.getAll();
      const isFirstUser = employees.length === 0;
      
      if (isFirstUser) {
        console.log('First user detected, setting as admin');
        
        // Set user metadata to include admin role - USE CORRECT FORMAT
        const clerk = await clerkClient();
        await clerk.users.updateUser(id, {
          publicMetadata: {
            role: "admin"
          }
        });
        
        // Add the user to the employees table
        const today = new Date().toISOString();
        await employeeQueries.add({
          name: `${first_name || ''} ${last_name || ''}`.trim() || 'Admin User',
          email: email,
          position: 'Admin',
          department: 'Administration',
          joinDate: today,
          status: 'Active',
          clerkId: id,
        });
        
        return NextResponse.json({ success: true, isAdmin: true });
      } else {
        // For subsequent users, we won't automatically add them to the database
        // The admin will need to add them manually
        console.log('User created but not added to database (admin needs to add manually)');
        
        return NextResponse.json({ success: true, isAdmin: false });
      }
    } catch (error) {
      console.error('Error processing user.created webhook:', error);
      return new Response('Error processing webhook', { status: 500 });
    }
  }
  
  // Return a 200 response for all other event types
  return NextResponse.json({ success: true });
} 