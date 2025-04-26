"use client"

import { useState, useEffect } from "react"

/**
 * Hook to detect if the current viewport is mobile-sized
 * @param breakpoint The width threshold in pixels to consider as mobile (default: 768)
 * @returns boolean indicating if the current viewport is mobile-sized
 */
export function useMobile(breakpoint = 768): boolean {
  const [isMobile, setIsMobile] = useState<boolean>(false)

  useEffect(() => {
    // Set initial value
    setIsMobile(window.innerWidth < breakpoint)

    // Handler to call on window resize
    function handleResize() {
      setIsMobile(window.innerWidth < breakpoint)
    }

    // Add event listener
    window.addEventListener("resize", handleResize)

    // Remove event listener on cleanup
    return () => window.removeEventListener("resize", handleResize)
  }, [breakpoint])

  return isMobile
}
