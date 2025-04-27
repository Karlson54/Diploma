import { clerkMiddleware, createRouteMatcher } from "@clerk/nextjs/server";

// Define public routes that don't require authentication
const isPublicRoute = createRouteMatcher([
  "/",
  "/login(.*)",
  "/signup(.*)",
  "/forgot-password(.*)"
]);

export default clerkMiddleware(async (auth, req) => {
  if (!isPublicRoute(req)) {
    // Protect all routes that are not public
    await auth.protect();
  }
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