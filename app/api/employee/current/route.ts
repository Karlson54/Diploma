import { NextResponse } from 'next/server';
import { employeeQueries } from '@/db/queries';
import { auth } from '@clerk/nextjs/server';

export async function GET() {
  try {
    // Get the Clerk user ID from the session
    const { userId } = await auth();
    
    if (!userId) {
      return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
    }
    
    // Find the employee with this Clerk ID
    const employee = await employeeQueries.getByClerkId(userId);
    
    if (!employee) {
      return NextResponse.json({ error: 'Employee not found' }, { status: 404 });
    }
    
    return NextResponse.json({ employee });
  } catch (error) {
    console.error('Error fetching current employee:', error);
    return NextResponse.json(
      { error: 'Internal server error' },
      { status: 500 }
    );
  }
} 