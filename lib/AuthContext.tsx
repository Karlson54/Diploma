"use client";

import { createContext, useContext, useState, useEffect, ReactNode } from "react";
import { useRouter } from "next/navigation";
import { useAuth, useUser } from "@clerk/nextjs";

interface AuthContextType {
  isAuthenticated: boolean;
  isLoading: boolean;
  isAdmin: boolean;
  user: any | null;
  redirectToLogin: () => void;
  redirectToDashboard: () => void;
}

const AuthContext = createContext<AuthContextType>({
  isAuthenticated: false,
  isLoading: true,
  isAdmin: false,
  user: null,
  redirectToLogin: () => {},
  redirectToDashboard: () => {},
});

export const useAuthContext = () => useContext(AuthContext);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const router = useRouter();
  const { isLoaded, userId, isSignedIn } = useAuth();
  const { user } = useUser();
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isAdmin, setIsAdmin] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (isLoaded) {
      setIsAuthenticated(!!isSignedIn);
      setIsLoading(false);
      
      if (isSignedIn && user) {
        const userRole = user.publicMetadata?.role as string;
        setIsAdmin(userRole === "admin");
      }
    }
  }, [isLoaded, isSignedIn, user]);

  const redirectToLogin = () => {
    router.push("/login");
  };

  const redirectToDashboard = () => {
    // Always redirect to main dashboard regardless of admin status
    router.push("/dashboard");
  };

  return (
    <AuthContext.Provider
      value={{
        isAuthenticated,
        isLoading,
        isAdmin,
        user,
        redirectToLogin,
        redirectToDashboard,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

// HOC for protected routes
export function withAuth(Component: React.ComponentType) {
  return function ProtectedRoute(props: any) {
    const { isAuthenticated, isLoading, redirectToLogin } = useAuthContext();
    const router = useRouter();

    useEffect(() => {
      if (!isLoading && !isAuthenticated) {
        redirectToLogin();
      }
    }, [isLoading, isAuthenticated, redirectToLogin]);

    if (isLoading) {
      return <div>Loading...</div>; // Or your custom loading component
    }

    if (!isAuthenticated) {
      return null; // Don't render anything while redirecting
    }

    return <Component {...props} />;
  };
}

// HOC for admin-only routes
export function withAdmin(Component: React.ComponentType) {
  return function AdminRoute(props: any) {
    const { isAuthenticated, isAdmin, isLoading, redirectToDashboard } = useAuthContext();
    
    useEffect(() => {
      if (!isLoading && (!isAuthenticated || !isAdmin)) {
        redirectToDashboard();
      }
    }, [isLoading, isAuthenticated, isAdmin, redirectToDashboard]);

    if (isLoading) {
      return <div>Loading...</div>; // Or your custom loading component
    }

    if (!isAuthenticated || !isAdmin) {
      return null; // Don't render anything while redirecting
    }

    return <Component {...props} />;
  };
} 