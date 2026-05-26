"use client";
import React from "react";
import { useMonitor } from "../context/MonitorContext";

export default function Dashboard() {
  const { stats, recentLogs, toast, handleDeleteProject, projects, setActiveTab, setSelectedProject } = useMonitor();

  return (
    <div className="flex flex-col gap-6 animate-fade-in">
      {/* Stats Widgets */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        {/* Total Services */}
        <div className="bg-slate-900/40 border border-slate-800/80 rounded-3xl p-6 flex items-center justify-between backdrop-blur-md relative overflow-hidden group">
          <div className="flex flex-col gap-1 z-10">
            <p className="text-xs text-slate-400 font-bold uppercase tracking-wider">Tổng dịch vụ</p>
            <h3 className="text-3xl font-extrabold text-white">{stats?.totalServices ?? 0}</h3>
            <p className="text-[10px] text-indigo-400 font-semibold">Đang kích hoạt: {stats?.activeServices ?? 0}</p>
          </div>
          <div className="w-12 h-12 rounded-2xl bg-indigo-950/40 border border-indigo-500/20 flex items-center justify-center text-indigo-400 shadow-inner z-10">
            <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
            </svg>
          </div>
        </div>
        {/* Services Online */}
        <div className="bg-slate-900/40 border border-slate-800/80 rounded-3xl p-6 flex items-center justify-between backdrop-blur-md relative overflow-hidden group">
          <div className="flex flex-col gap-1 z-10">
            <p className="text-xs text-slate-400 font-bold uppercase tracking-wider">Đang hoạt động</p>
            <h3 className="text-3xl font-extrabold text-emerald-400">{stats?.onlineServices ?? 0}</h3>
            <p className="text-[10px] text-emerald-500 font-semibold">Trạng thái ONLINE</p>
          </div>
          <div className="w-12 h-12 rounded-2xl bg-emerald-950/40 border border-emerald-500/20 flex items-center justify-center text-emerald-400 shadow-inner z-10">
            <svg className="w-6 h-6 animate-pulse" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M5 13l4 4L19 7" />
            </svg>
          </div>
        </div>
        {/* Services Offline */}
        <div className="bg-slate-900/40 border border-slate-800/80 rounded-3xl p-6 flex items-center justify-between backdrop-blur-md relative overflow-hidden group">
          <div className="flex flex-col gap-1 z-10">
            <p className="text-xs text-slate-400 font-bold uppercase tracking-wider">Gặp sự cố</p>
            <h3 className={`text-3xl font-extrabold ${(stats?.offlineServices ?? 0) > 0 ? "text-rose-500" : "text-slate-400"}`}>{stats?.offlineServices ?? 0}</h3>
            <p className="text-[10px] text-rose-500/85 font-semibold">Trạng thái OFFLINE</p>
          </div>
          <div className={`w-12 h-12 rounded-2xl flex items-center justify-center shadow-inner z-10 border ${(stats?.offlineServices ?? 0) > 0 ? "bg-rose-950/40 border-rose-500/20 text-rose-500 animate-bounce" : "bg-slate-950/40 border-slate-800 text-slate-650"}`}>
            <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
          </div>
        </div>
        {/* Uptime % 24h */}
        <div className="bg-slate-900/40 border border-slate-800/80 rounded-3xl p-6 flex items-center justify-between backdrop-blur-md relative overflow-hidden group">
          <div className="flex flex-col gap-1 z-10">
            <p className="text-xs text-slate-400 font-bold uppercase tracking-wider">Tỷ lệ Uptime 24h</p>
            <h3 className="text-3xl font-extrabold text-indigo-400">{stats?.overallUptime24h ?? 100}%</h3>
            <p className="text-[10px] text-slate-500 font-semibold">Tình trạng hoạt động tổng thể</p>
          </div>
          <div className="w-12 h-12 rounded-2xl bg-indigo-950/40 border border-indigo-500/20 flex items-center justify-center text-indigo-400 shadow-inner z-10">
            <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
            </svg>
          </div>
        </div>
      </div>

      {/* Main Content Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        
        {/* Projects Overview */}
        <div className="lg:col-span-2 flex flex-col gap-4">
          <div className="flex items-center justify-between">
            <h3 className="text-lg font-bold text-white">Tổng quan Dự án</h3>
            <button
              onClick={() => setActiveTab("projects")}
              className="text-xs text-indigo-400 hover:text-indigo-300 transition-colors font-medium"
            >
              Xem tất cả &rarr;
            </button>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {projects.length === 0 ? (
              <div className="sm:col-span-2 bg-slate-900/40 border border-slate-800/80 rounded-3xl p-8 flex flex-col items-center justify-center text-center">
                <p className="text-sm text-slate-500 mb-4">Chưa có dự án nào được cấu hình</p>
                <button
                  onClick={() => setActiveTab("projects")}
                  className="px-4 py-2 bg-indigo-600 hover:bg-indigo-500 text-white rounded-xl text-xs font-bold transition-all shadow-lg shadow-indigo-600/20"
                >
                  Thêm dự án đầu tiên
                </button>
              </div>
            ) : (
              projects.slice(0, 4).map((proj: any) => (
                <div
                  key={proj.id}
                  onClick={() => {
                    setSelectedProject(proj);
                    setActiveTab("services");
                  }}
                  className="bg-slate-900/40 hover:bg-slate-800/60 border border-slate-800/80 hover:border-indigo-500/50 rounded-3xl p-5 flex flex-col gap-3 transition-all cursor-pointer group backdrop-blur-md"
                >
                  <div className="flex items-start justify-between">
                    <h4 className="text-base font-bold text-white group-hover:text-indigo-300 transition-colors line-clamp-1">{proj.name}</h4>
                    <span className="text-[10px] font-black uppercase tracking-wider px-2 py-1 bg-emerald-950/40 text-emerald-400 rounded-md border border-emerald-500/20">
                      {proj.status}
                    </span>
                  </div>
                  <p className="text-xs text-slate-400 line-clamp-2 min-h-[2rem]">
                    {proj.description || "Không có mô tả"}
                  </p>
                  <div className="flex items-center justify-between mt-1 pt-3 border-t border-slate-800/50">
                    <span className="text-[10px] text-slate-500 font-medium">Click để xem dịch vụ</span>
                    <svg className="w-4 h-4 text-slate-600 group-hover:text-indigo-400 transition-colors transform group-hover:translate-x-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M14 5l7 7m0 0l-7 7m7-7H3" />
                    </svg>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>

        {/* Recent Logs Sidebar */}
        <div className="lg:col-span-1 bg-slate-900/40 border border-slate-800/80 rounded-3xl p-6 backdrop-blur-md flex flex-col">
          <div className="flex items-center justify-between mb-6">
            <h3 className="text-lg font-bold text-white flex items-center gap-2">
              <span className="relative flex h-3 w-3">
                <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-emerald-400 opacity-75"></span>
                <span className="relative inline-flex rounded-full h-3 w-3 bg-emerald-500"></span>
              </span>
              Log Thời gian thực
            </h3>
          </div>
          <div className="flex-1 overflow-y-auto pr-2 space-y-3 custom-scrollbar" style={{ maxHeight: "400px" }}>
            {recentLogs.length === 0 ? (
              <div className="h-full flex items-center justify-center">
                <p className="text-xs text-slate-500 text-center py-6">Không có log sự kiện nào.</p>
              </div>
            ) : (
              recentLogs.map((log: any) => {
                const logTime = new Date(log.checkedAt).toLocaleTimeString("vi-VN");
                const isOnline = log.status === 1;
                return (
                  <div
                    key={log.id}
                    className={`bg-slate-950/80 border p-3.5 rounded-2xl flex flex-col gap-2 transition-all hover:-translate-y-0.5 ${
                      isOnline ? "border-emerald-500/10 hover:border-emerald-500/30" : "border-rose-500/20 hover:border-rose-500/40"
                    }`}
                  >
                    <div className="flex items-center justify-between gap-2">
                      <div className="flex flex-col gap-0.5 max-w-[150px]">
                        <div className="flex items-center gap-2">
                          <span className={`w-2 h-2 rounded-full ${isOnline ? "bg-emerald-500 shadow-[0_0_8px_rgba(16,185,129,0.8)]" : "bg-rose-500 shadow-[0_0_8px_rgba(244,63,94,0.8)] animate-pulse"}`} />
                          <span className="text-[11px] font-bold text-slate-200 truncate" title={log.serviceName || log.serviceId}>
                            {log.serviceName || log.serviceId.substring(log.serviceId.length - 8)}
                          </span>
                        </div>
                        {log.projectName && (
                          <span className="text-[9px] text-slate-400 truncate ml-4" title={log.projectName}>
                            {log.projectName}
                          </span>
                        )}
                      </div>
                      <span className={`text-[9px] px-2 py-0.5 rounded-md font-bold uppercase ${isOnline ? "bg-emerald-950/50 text-emerald-400" : "bg-rose-950/50 text-rose-400"}`}>
                        {isOnline ? "Online" : "Offline"}
                      </span>
                    </div>
                    
                    <div className="flex items-center justify-between">
                      <span className="text-[10px] font-mono font-medium text-slate-400 bg-slate-900 px-1.5 py-0.5 rounded">HTTP {log.statusCode}</span>
                      <span className="text-[10px] font-bold text-slate-300 font-mono">{log.responseTimeMs} ms</span>
                    </div>

                    {!isOnline && log.errorMassage && (
                      <p className="text-[10px] text-rose-400/90 font-medium bg-rose-950/30 p-1.5 rounded flex-1 mt-1">
                        {log.errorMassage}
                      </p>
                    )}
                    
                    <div className="text-[9px] text-slate-500 font-medium text-right mt-1">
                      {logTime}
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
