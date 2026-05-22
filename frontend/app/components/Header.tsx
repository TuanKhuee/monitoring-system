"use client";
import React from "react";
import { useMonitor } from "../context/MonitorContext";

export default function Header() {
  const { user, handleLogout, activeTab, setActiveTab } = useMonitor();

  const tabs = [
    { id: "dashboard", label: "Dashboard" },
    { id: "projects", label: "Dự án" },
    { id: "services", label: "Dịch vụ" },
  ];

  return (
    <header className="sticky top-0 z-40 bg-slate-900/60 backdrop-blur-xl border-b border-slate-800/80 px-6 py-4 flex flex-col md:flex-row items-center justify-between gap-4">
      <div className="flex items-center gap-3">
        <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-indigo-600 to-violet-500 flex items-center justify-center shadow-lg shadow-indigo-500/20">
          <svg className="w-6 h-6 text-white animate-pulse" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 002 2h2a2 2 0 002-2z" />
          </svg>
        </div>
        <div>
          <h1 className="text-xl font-bold tracking-tight bg-gradient-to-r from-indigo-300 via-violet-200 to-indigo-100 bg-clip-text text-transparent">
            Monitoring
          </h1>
          <p className="text-xs text-slate-400 font-medium">Production Website & Service Monitor</p>
        </div>
      </div>

      {/* Navigation Tabs */}
      <nav className="flex gap-4">
        {tabs.map((tab) => (
          <button
            key={tab.id}
            onClick={() => setActiveTab(tab.id)}
            className={`px-3 py-1 rounded-md text-sm font-medium transition-colors ${activeTab === tab.id ? "bg-indigo-800 text-white" : "text-slate-300 hover:bg-slate-800 hover:text-white"}`}
          >
            {tab.label}
          </button>
        ))}
      </nav>

      {user && (
        <div className="flex items-center gap-3">
          <div className="text-right">
            <p className="text-xs font-bold text-slate-200">{user.username}</p>
            <p className="text-[10px] font-semibold text-indigo-400 uppercase tracking-widest">{user.role}</p>
          </div>
          <button
            onClick={handleLogout}
            className="p-2.5 rounded-xl bg-slate-800/60 text-slate-400 hover:text-rose-400 hover:bg-rose-950/20 border border-slate-700/50 hover:border-rose-900/30 transition-all"
            title="Đăng xuất"
          >
            <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
            </svg>
          </button>
        </div>
      )}
    </header>
  );
}
