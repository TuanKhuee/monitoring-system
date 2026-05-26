import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Allow fetching resources from the local IP address
  allowedDevOrigins: ['192.168.0.240'],
};

export default nextConfig;
