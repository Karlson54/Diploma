import { clerkMiddleware, createRouteMatcher } from "@clerk/nextjs/server";
import { NextResponse } from "next/server";

// Define public routes that don't require authentication
const isPublicRoute = createRouteMatcher([
  "/login(.*)",
  "/signup(.*)",
  "/forgot-password(.*)",
  "/api/webhook",
  "/api/auth/registration-status",
  "/api/auth/set-admin-role",
  "/api/auth/first-user"
]);

export default clerkMiddleware(async (auth, req) => {
  if (isPublicRoute(req)) {
    // Allow access to public routes
    return NextResponse.next();
  }
  
  // For all other routes, require authentication
  await auth.protect();
});

// Apply the middleware to specific routes
export const config = {
  matcher: [
    '/',
    '/dashboard/:path*',
    '/admin/:path*',
    '/api/:path*',
    '/reports/:path*',
    '/profile/:path*',
    '/login',
    '/signup',
    '/forgot-password'
  ],
}; 