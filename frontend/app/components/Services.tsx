"use client";
import React, { useState } from "react";
import { useMonitor } from "../context/MonitorContext";

const PROTOCOLS = ["Http", "Https", "Tcp", "Ping"];

const defaultServiceForm = {
  serviceName: "",
  ip: "",
  port: 80,
  protocol: "Http",
  healthEndpoint: "/",
  intervalSeconds: 10,
  isActive: true,
};

export default function Services() {
  const {
    services,
    projects,
    selectedProject,
    setSelectedProject,
    fetchServices,
    setActiveTab,
    user,
    handleCreateService,
    handleUpdateService,
    handleDeleteService,
    serviceForm,
    setServiceForm,
  } = useMonitor();

  const [showModal, setShowModal] = useState(false);
  const [isEditMode, setIsEditMode] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const openCreateModal = () => {
    setIsEditMode(false);
    setEditingId(null);
    setServiceForm({ ...defaultServiceForm });
    setShowModal(true);
  };

  const openEditModal = (svc: any) => {
    setIsEditMode(true);
    setEditingId(svc.id);
    setServiceForm({
      serviceName: svc.serviceName,
      ip: svc.ip,
      port: svc.port,
      protocol: svc.protocol,
      healthEndpoint: svc.healthEndpoint ?? "/",
      intervalSeconds: svc.intervalSeconds,
      isActive: svc.isActive,
    });
    setShowModal(true);
  };

  const submitForm = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    try {
      if (isEditMode && editingId) {
        await handleUpdateService(editingId, serviceForm);
      } else {
        await handleCreateService(e);
      }
      setShowModal(false);
    } finally {
      setIsSubmitting(false);
    }
  };

  const protocolBadgeColor = (protocol: string) => {
    switch (protocol.toLowerCase()) {
      case "http": return "bg-sky-950/50 text-sky-400 border border-sky-500/20";
      case "https": return "bg-emerald-950/50 text-emerald-400 border border-emerald-500/20";
      case "tcp": return "bg-violet-950/50 text-violet-400 border border-violet-500/20";
      case "ping": return "bg-amber-950/50 text-amber-400 border border-amber-500/20";
      default: return "bg-slate-800 text-slate-400";
    }
  };

  const showHealthEndpoint = ["Http", "Https"].includes(serviceForm.protocol);

  return (
    <div className="flex flex-col gap-6 animate-fade-in">

      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h2 className="text-2xl font-bold text-white">Quản lý Dịch vụ</h2>
          <p className="text-sm text-slate-400 mt-1">
            {selectedProject
              ? `Dịch vụ của dự án: ${selectedProject.name}`
              : "Chọn một dự án để xem danh sách dịch vụ"}
          </p>
        </div>
        {selectedProject && (
          <button
            onClick={openCreateModal}
            className="flex items-center gap-2 bg-gradient-to-r from-indigo-500 to-violet-600 hover:from-indigo-400 hover:to-violet-500 text-white px-5 py-2.5 rounded-xl text-sm font-bold shadow-lg shadow-indigo-500/25 transition-all transform hover:-translate-y-0.5"
          >
            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M12 4v16m8-8H4" />
            </svg>
            Thêm dịch vụ
          </button>
        )}
      </div>

      {/* Project Selector */}
      <div className="bg-slate-900/40 border border-slate-800/80 rounded-2xl p-4 backdrop-blur-md">
        <p className="text-xs font-bold text-slate-400 uppercase tracking-wider mb-3">Chọn Dự án</p>
        <div className="flex flex-wrap gap-2">
          {projects.length === 0 ? (
            <p className="text-sm text-slate-500">Chưa có dự án nào.{" "}
              <button onClick={() => setActiveTab("projects")} className="text-indigo-400 hover:underline">
                Tạo dự án đầu tiên →
              </button>
            </p>
          ) : (
            projects.map((proj: any) => (
              <button
                key={proj.id}
                onClick={() => {
                  setSelectedProject(proj);
                  fetchServices(proj.id);
                }}
                className={`px-4 py-2 rounded-xl text-sm font-semibold transition-all border ${
                  selectedProject?.id === proj.id
                    ? "bg-indigo-600/30 border-indigo-500 text-indigo-200"
                    : "bg-slate-800/50 border-slate-700/50 text-slate-300 hover:border-indigo-500/50 hover:text-white"
                }`}
              >
                {proj.name}
              </button>
            ))
          )}
        </div>
      </div>

      {/* Service List */}
      {!selectedProject ? (
        <div className="bg-slate-900/40 border border-dashed border-slate-700 rounded-3xl p-16 flex flex-col items-center gap-4 text-center">
          <div className="w-16 h-16 rounded-2xl bg-slate-800 flex items-center justify-center">
            <svg className="w-8 h-8 text-slate-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
            </svg>
          </div>
          <p className="text-slate-400 font-medium">Chọn một dự án để xem các dịch vụ đang giám sát</p>
        </div>
      ) : services.length === 0 ? (
        <div className="bg-slate-900/40 border border-dashed border-slate-700 rounded-3xl p-16 flex flex-col items-center gap-4 text-center">
          <p className="text-slate-400">Chưa có dịch vụ nào trong dự án <strong className="text-white">{selectedProject.name}</strong></p>
          <button
            onClick={openCreateModal}
            className="px-5 py-2.5 bg-indigo-600 hover:bg-indigo-500 text-white rounded-xl text-sm font-bold transition-all"
          >
            + Thêm dịch vụ đầu tiên
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          {services.map((svc: any) => (
            <div
              key={svc.id}
              className={`bg-slate-900/40 border rounded-3xl p-5 flex flex-col gap-4 backdrop-blur-md transition-all hover:-translate-y-0.5 ${
                svc.isActive ? "border-slate-800/80 hover:border-indigo-500/40" : "border-slate-800/40 opacity-60"
              }`}
            >
              {/* Service Header */}
              <div className="flex items-start justify-between gap-2">
                <div className="flex items-center gap-3">
                  <div className={`w-3 h-3 rounded-full flex-shrink-0 ${svc.isActive ? "bg-emerald-500 shadow-[0_0_8px_rgba(16,185,129,0.6)]" : "bg-slate-600"}`} />
                  <div>
                    <h3 className="text-base font-bold text-white">{svc.serviceName}</h3>
                    <p className="text-xs text-slate-400 font-mono mt-0.5">{svc.ip}:{svc.port}{svc.healthEndpoint || ""}</p>
                  </div>
                </div>
                <span className={`text-[10px] px-2.5 py-1 rounded-lg font-black uppercase tracking-wider ${protocolBadgeColor(svc.protocol)}`}>
                  {svc.protocol}
                </span>
              </div>

              {/* Service Details */}
              <div className="grid grid-cols-2 gap-2">
                <div className="bg-slate-950/50 rounded-xl p-3">
                  <p className="text-[10px] text-slate-500 font-bold uppercase tracking-wider">Chu kỳ kiểm tra</p>
                  <p className="text-sm text-white font-bold mt-1">{svc.intervalSeconds}s</p>
                </div>
                <div className="bg-slate-950/50 rounded-xl p-3">
                  <p className="text-[10px] text-slate-500 font-bold uppercase tracking-wider">Trạng thái</p>
                  <p className={`text-sm font-bold mt-1 ${svc.isActive ? "text-emerald-400" : "text-slate-500"}`}>
                    {svc.isActive ? "Đang giám sát" : "Đã tắt"}
                  </p>
                </div>
              </div>

              {/* Actions */}
              <div className="flex items-center gap-2 pt-2 border-t border-slate-800/50">
                <button
                  onClick={() => openEditModal(svc)}
                  className="flex-1 py-2 text-xs font-bold text-indigo-400 hover:text-white hover:bg-indigo-600/20 rounded-xl transition-all border border-transparent hover:border-indigo-500/30"
                >
                  Chỉnh sửa
                </button>
                <button
                  onClick={() => handleDeleteService(svc.id)}
                  className="flex-1 py-2 text-xs font-bold text-rose-400 hover:text-white hover:bg-rose-600/20 rounded-xl transition-all border border-transparent hover:border-rose-500/30"
                >
                  Xóa dịch vụ
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Create / Edit Modal */}
      {showModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/70 backdrop-blur-sm p-4">
          <div className="bg-slate-900 border border-slate-700/80 rounded-3xl p-8 w-full max-w-xl shadow-2xl flex flex-col gap-6 max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between">
              <h3 className="text-xl font-bold text-white">
                {isEditMode ? "Chỉnh sửa Dịch vụ" : "Thêm Dịch vụ mới"}
              </h3>
              <button
                onClick={() => setShowModal(false)}
                className="w-8 h-8 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-400 hover:text-white transition-all flex items-center justify-center text-lg font-bold"
              >
                ×
              </button>
            </div>

            <form onSubmit={submitForm} className="flex flex-col gap-4">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                {/* Service Name */}
                <div className="flex flex-col gap-1.5 sm:col-span-2">
                  <label className="text-xs font-bold text-slate-400 uppercase tracking-wider ml-1">
                    Tên dịch vụ <span className="text-rose-500">*</span>
                  </label>
                  <input
                    type="text"
                    required
                    placeholder="VD: API Gateway, Frontend Web..."
                    value={serviceForm.serviceName}
                    onChange={(e) => setServiceForm({ ...serviceForm, serviceName: e.target.value })}
                    className="bg-slate-950/50 border border-slate-700/50 text-white px-4 py-3 rounded-xl focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all text-sm placeholder:text-slate-600"
                  />
                </div>

                {/* Protocol */}
                <div className="flex flex-col gap-1.5">
                  <label className="text-xs font-bold text-slate-400 uppercase tracking-wider ml-1">
                    Giao thức <span className="text-rose-500">*</span>
                  </label>
                  <select
                    value={serviceForm.protocol}
                    onChange={(e) => setServiceForm({ ...serviceForm, protocol: e.target.value })}
                    className="bg-slate-950/50 border border-slate-700/50 text-white px-4 py-3 rounded-xl focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all text-sm"
                  >
                    {PROTOCOLS.map(p => <option key={p} value={p}>{p}</option>)}
                  </select>
                </div>

                {/* IP / Hostname */}
                <div className="flex flex-col gap-1.5">
                  <label className="text-xs font-bold text-slate-400 uppercase tracking-wider ml-1">
                    IP / Hostname <span className="text-rose-500">*</span>
                  </label>
                  <input
                    type="text"
                    required
                    placeholder="VD: 192.168.1.1 hoặc example.com"
                    value={serviceForm.ip}
                    onChange={(e) => setServiceForm({ ...serviceForm, ip: e.target.value })}
                    className="bg-slate-950/50 border border-slate-700/50 text-white px-4 py-3 rounded-xl focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all text-sm placeholder:text-slate-600"
                  />
                </div>

                {/* Port */}
                <div className="flex flex-col gap-1.5">
                  <label className="text-xs font-bold text-slate-400 uppercase tracking-wider ml-1">
                    Cổng (Port) <span className="text-rose-500">*</span>
                  </label>
                  <input
                    type="number"
                    required
                    min={1}
                    max={65535}
                    placeholder="80"
                    value={serviceForm.port}
                    onChange={(e) => setServiceForm({ ...serviceForm, port: Number(e.target.value) })}
                    className="bg-slate-950/50 border border-slate-700/50 text-white px-4 py-3 rounded-xl focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all text-sm placeholder:text-slate-600"
                  />
                </div>

                {/* Interval */}
                <div className="flex flex-col gap-1.5">
                  <label className="text-xs font-bold text-slate-400 uppercase tracking-wider ml-1">
                    Chu kỳ kiểm tra (giây)
                  </label>
                  <input
                    type="number"
                    min={10}
                    max={3600}
                    value={serviceForm.intervalSeconds}
                    onChange={(e) => setServiceForm({ ...serviceForm, intervalSeconds: Number(e.target.value) })}
                    className="bg-slate-950/50 border border-slate-700/50 text-white px-4 py-3 rounded-xl focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all text-sm"
                  />
                </div>

                {/* Health Endpoint (only for HTTP/HTTPS) */}
                {showHealthEndpoint && (
                  <div className="flex flex-col gap-1.5 sm:col-span-2">
                    <label className="text-xs font-bold text-slate-400 uppercase tracking-wider ml-1">
                      Health Check Endpoint
                    </label>
                    <input
                      type="text"
                      placeholder="/health hoặc /"
                      value={serviceForm.healthEndpoint}
                      onChange={(e) => setServiceForm({ ...serviceForm, healthEndpoint: e.target.value })}
                      className="bg-slate-950/50 border border-slate-700/50 text-white px-4 py-3 rounded-xl focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all text-sm placeholder:text-slate-600"
                    />
                  </div>
                )}

                {/* IsActive Toggle */}
                <div className="flex items-center justify-between bg-slate-950/50 border border-slate-700/50 rounded-xl px-4 py-3 sm:col-span-2">
                  <div>
                    <p className="text-sm font-bold text-white">Kích hoạt giám sát</p>
                    <p className="text-xs text-slate-500">Dịch vụ sẽ được kiểm tra theo chu kỳ đã đặt</p>
                  </div>
                  <button
                    type="button"
                    onClick={() => setServiceForm({ ...serviceForm, isActive: !serviceForm.isActive })}
                    className={`relative w-12 h-6 rounded-full transition-all ${serviceForm.isActive ? "bg-indigo-500" : "bg-slate-700"}`}
                  >
                    <span className={`absolute top-1 left-1 w-4 h-4 bg-white rounded-full shadow transition-transform ${serviceForm.isActive ? "translate-x-6" : ""}`} />
                  </button>
                </div>
              </div>

              {/* Submit */}
              <div className="flex justify-end gap-3 mt-2 pt-4 border-t border-slate-800">
                <button
                  type="button"
                  onClick={() => setShowModal(false)}
                  className="px-5 py-2.5 text-sm text-slate-400 hover:text-white transition-colors"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  disabled={isSubmitting}
                  className="px-6 py-2.5 bg-gradient-to-r from-indigo-500 to-violet-600 hover:from-indigo-400 hover:to-violet-500 disabled:opacity-60 text-white rounded-xl text-sm font-bold transition-all shadow-lg shadow-indigo-500/25 flex items-center gap-2"
                >
                  {isSubmitting ? (
                    <><svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"/></svg> Đang lưu...</>
                  ) : (isEditMode ? "Cập nhật" : "Thêm dịch vụ")}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
