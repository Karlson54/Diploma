import { NextResponse } from 'next/server';
import { clerkClient } from '@clerk/nextjs/server';

// This endpoint allows setting a user as admin by providing their user ID
// POST /api/auth/set-admin-role
// Body: { userId: "user_123" }
export async function POST(req: Request) {
  try {
    const { userId } = await req.json();
    
    if (!userId) {
      return NextResponse.json({ error: 'Missing userId parameter' }, { status: 400 });
    }
    
    // Set the admin role for the user
    const clerk = await clerkClient();
    await clerk.users.updateUser(userId, {
      publicMetadata: {
        role: "admin"
      }
    });
    
    return NextResponse.json({ 
      success: true, 
      message: 'User role set to admin successfully',
      userId
    });
  } catch (error) {
    console.error('Error setting admin role:', error);
    return NextResponse.json(
      { error: 'Failed to set admin role' },
      { status: 500 }
    );
  }
} 