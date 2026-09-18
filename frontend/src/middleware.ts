import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

/**
 * Route protection middleware inspecting auth_token cookie for private management routes.
 * Redirects unauthenticated visitors to /auth with preserved returnUrl.
 * Also forwards already authenticated visitors from /auth to /dashboard or returnUrl.
 */
export function middleware(request: NextRequest) {
  const { pathname, search } = request.nextUrl;
  const token = request.cookies.get('auth_token')?.value;

  // Protected management routes
  if (pathname.startsWith('/dashboard') || pathname.startsWith('/management')) {
    if (!token || token.trim() === '') {
      const returnUrl = encodeURIComponent(`${pathname}${search}`);
      const loginUrl = new URL(`/auth?returnUrl=${returnUrl}`, request.url);
      return NextResponse.redirect(loginUrl);
    }
  }

  // Redirect logged-in users away from /auth to their destination or dashboard
  if (pathname === '/auth') {
    if (token && token.trim() !== '') {
      const returnUrlParam = request.nextUrl.searchParams.get('returnUrl');
      let targetPath = '/dashboard';
      if (returnUrlParam) {
        const decoded = decodeURIComponent(returnUrlParam);
        if (!decoded.startsWith('/auth')) {
          targetPath = decoded;
        }
      }
      return NextResponse.redirect(new URL(targetPath, request.url));
    }
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/dashboard/:path*', '/management/:path*', '/auth'],
};
