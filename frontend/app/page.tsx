"use client";
import Header from "./components/Header";
import ToastAlert from "./components/ToastAlert";
import Dashboard from "./components/Dashboard";
import Projects from "./components/Projects";
import Services from "./components/Services";
import { useMonitor } from "./context/MonitorContext";
import Link from "next/link";

export default function Home() {
  const {
    activeTab,
    setActiveTab,
    token,
    // other state not needed here; UI for auth moved to /auth
  } = useMonitor();

  // If not authenticated, prompt to go to login page
  if (!token) {
    return (
      <div className="flex flex-1 items-center justify-center">
        <Link href="/auth" className="text-indigo-400 hover:underline">
          Vui lòng đăng nhập để tiếp tục
        </Link>
      </div>
    );
  }

  return (
    <div className="flex flex-col min-h-screen bg-slate-950 text-slate-100 font-sans selection:bg-indigo-500 selection:text-white">
      {/* Global toast notifications */}
      <ToastAlert />
      {/* Header includes navigation tabs */}
      <Header />
      {/* Main container – show component based on active tab */}
      <main className="flex-1 flex flex-col p-6 max-w-7xl w-full mx-auto gap-6">
        {activeTab === "dashboard" && <Dashboard />}
        {activeTab === "projects" && <Projects />}
        {activeTab === "services" && <Services />}
      </main>
    </div>
  );
}
