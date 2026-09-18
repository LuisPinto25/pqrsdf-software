'use client';

import { useState, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import type { LoginCredentials, UserProfile, AuthState } from './auth.types';
import { getAuthToken, setAuthToken, removeAuthToken, loginStaff } from '@/shared/api/client';

export function useAuth() {
  const router = useRouter();

  const [state, setState] = useState<AuthState>({
    user: null,
    token: null,
    isAuthenticated: false,
    isLoading: true,
  });

  useEffect(() => {
    const token = getAuthToken();
    let user: UserProfile | null = null;
    try {
      const stored = localStorage.getItem('auth_user');
      if (stored) {
        user = JSON.parse(stored) as UserProfile;
      }
    } catch {
      // Ignore parse failure
    }

    if (token && user) {
      setState({
        user,
        token,
        isAuthenticated: true,
        isLoading: false,
      });
    } else {
      setState({
        user: null,
        token: null,
        isAuthenticated: false,
        isLoading: false,
      });
    }
  }, []);

  const login = useCallback(
    async (
      credentials: LoginCredentials,
      destination?: string
    ): Promise<{ isSuccess: boolean; error?: string }> => {
      setState((prev) => ({ ...prev, isLoading: true }));
      const result = await loginStaff(credentials);

      if (result.ok) {
        const { accessToken, expiresIn, user } = result.value;
        setAuthToken(accessToken, expiresIn);
        try {
          localStorage.setItem('auth_user', JSON.stringify(user));
        } catch {
          // Ignore storage quota error
        }

        setState({
          user,
          token: accessToken,
          isAuthenticated: true,
          isLoading: false,
        });

        let target = '/dashboard';
        if (destination) {
          const decoded = decodeURIComponent(destination);
          if (!decoded.startsWith('/auth')) {
            target = decoded;
          }
        }

        if (typeof window !== 'undefined') {
          window.location.href = target;
        } else {
          router.push(target);
        }
        return { isSuccess: true };
      }

      setState((prev) => ({ ...prev, isLoading: false }));
      return { isSuccess: false, error: result.error.message };
    },
    [router]
  );

  const logout = useCallback(() => {
    removeAuthToken();
    try {
      localStorage.removeItem('auth_user');
    } catch {
      // Ignore
    }
    setState({
      user: null,
      token: null,
      isAuthenticated: false,
      isLoading: false,
    });
    if (typeof window !== 'undefined') {
      window.location.href = '/?loggedOut=true';
    } else {
      router.replace('/?loggedOut=true');
    }
  }, [router]);

  return {
    ...state,
    login,
    logout,
  };
}
