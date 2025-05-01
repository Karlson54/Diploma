"use client";

import { useState, useEffect } from "react";
import { useUser } from "@clerk/nextjs";

interface Employee {
  id: number;
  name: string;
  email: string;
  position: string;
  department: string;
  joinDate: string;
  status: string;
  clerkId: string | null;
  agency: string | null;
}

export function useCurrentEmployee() {
  const { user, isLoaded } = useUser();
  const [employee, setEmployee] = useState<Employee | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function fetchEmployeeData() {
      if (!isLoaded || !user) {
        setIsLoading(false);
        return;
      }

      try {
        const response = await fetch('/api/employee/current');
        
        if (!response.ok) {
          throw new Error('Failed to fetch employee data');
        }

        const data = await response.json();
        setEmployee(data.employee);
      } catch (err) {
        console.error('Error fetching employee data:', err);
        setError('Could not load employee data');
      } finally {
        setIsLoading(false);
      }
    }

    fetchEmployeeData();
  }, [isLoaded, user]);

  return { employee, isLoading, error };
} 