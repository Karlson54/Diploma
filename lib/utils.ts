import { type ClassValue, clsx } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

/**
 * Format employee name for display
 * 
 * @param fullName Full name of the employee from the database
 * @returns An object with firstName and lastName properties
 */
export function formatEmployeeName(fullName: string) {
  if (!fullName) return { firstName: '', lastName: '' };
  
  const nameParts = fullName.trim().split(' ');
  
  if (nameParts.length === 1) {
    return { firstName: nameParts[0], lastName: '' };
  }
  
  const firstName = nameParts[0];
  const lastName = nameParts.slice(1).join(' ');
  
  return { firstName, lastName };
}
