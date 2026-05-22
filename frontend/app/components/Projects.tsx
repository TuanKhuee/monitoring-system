"use client";
import React, { useState } from "react";
import { useMonitor } from "../context/MonitorContext";

export default function Projects() {
  const {
    projects,
    selectedProject,
    setSelectedProject,
    handleCreateProject,
    handleEditProject,
    handleDeleteProject,
    projectForm,
    setProjectForm,
  } = useMonitor();

  const [showModal, setShowModal] = useState(false);
  const [isEditMode, setIsEditMode] = useState(false);

  const openCreateModal = () => {
    setIsEditMode(false);
    setProjectForm({
      id: "",
      name: "",
      description: "",
      projectCode: "",
      projectUrl: "",
      repositoryUrl: "",
      status: "Active",
    });
    setShowModal(true);
  };

  const openEditModal = (proj: any) => {
    setIsEditMode(true);
    setProjectForm({
      id: proj.id,
      name: proj.name,
      description: proj.description,
      projectCode: proj.projectCode,
      projectUrl: proj.projectUrl,
      repositoryUrl: proj.repositoryUrl,
      status: proj.status,
    });
    setShowModal(true);
  };

  const submitForm = async (e: React.FormEvent) => {
    e.preventDefault();
    await handleCreateProject(e);
    setShowModal(false);
  };

  return (
    <div className="flex flex-col gap-6 animate-fade-in">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-bold text-white">Quản lý Dự án</h2>
        <button
          className="bg-indigo-600 hover:bg-indigo-500 text-white py-2 px-4 rounded-lg shadow"
          onClick={openCreateModal}
        >
          + Thêm Dự án
        </button>
      </div>

      {/* Project list */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {projects.map((proj) => (
          <div
            key={proj.id}
            className="bg-slate-900/40 border border-slate-800/80 rounded-2xl p-4 flex flex-col gap-2 cursor-pointer hover:border-indigo-500 transition"
            onClick={() => setSelectedProject(proj)}
          >
            <h3 className="text-lg font-semibold text-white">{proj.name}</h3>
            <p className="text-xs text-slate-400">{proj.description}</p>
            <div className="flex gap-2 mt-2">
              <button
                className="text-sm text-indigo-400 hover:underline"
                onClick={(e) => { e.stopPropagation(); openEditModal(proj); }}
              >
                Sửa
              </button>
              <button
                className="text-sm text-rose-400 hover:underline"
                onClick={(e) => { e.stopPropagation(); handleDeleteProject(proj.id); }}
              >
                Xóa
              </button>
            </div>
          </div>
        ))}
      </div>

      {/* Modal for create / edit */}
      {showModal && (
        <div className="fixed inset-0 flex items-center justify-center bg-black/60 backdrop-blur-sm">
          <div className="bg-slate-900/90 border border-slate-800/80 rounded-2xl p-6 w-full max-w-lg">
            <h3 className="text-lg font-bold text-white mb-4">
              {isEditMode ? "Cập nhật Dự án" : "Thêm Dự án mới"}
            </h3>
            <form onSubmit={submitForm} className="flex flex-col gap-4">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="flex flex-col gap-1.5 md:col-span-2">
                  <label className="text-xs font-bold text-slate-400 uppercase tracking-wider ml-1">Tên dự án <span className="text-rose-500">*</span></label>
                  <input
                    type="text"
                    placeholder="VD: Cổng thông tin nội bộ..."
                    value={projectForm.name}
                    onChange={(e) => setProjectForm({ ...projectForm, name: e.target.value })}
                    className="bg-slate-950/50 border border-slate-700/50 text-white px-4 py-3 rounded-xl focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all text-sm placeholder:text-slate-600"
                    required
                  />
                </div>

                <div className="flex flex-col gap-1.5">
                  <label className="text-xs font-bold text-slate-400 uppercase tracking-wider ml-1">Mã dự án <span className="text-rose-500">*</span></label>
                  <input
                    type="text"
                    placeholder="VD: PORTAL-01"
                    value={projectForm.projectCode}
                    onChange={(e) => setProjectForm({ ...projectForm, projectCode: e.target.value })}
                    className="bg-slate-950/50 border border-slate-700/50 text-white px-4 py-3 rounded-xl focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all text-sm placeholder:text-slate-600"
                    required
                  />
                </div>

                <div className="flex flex-col gap-1.5">
                  <label className="text-xs font-bold text-slate-400 uppercase tracking-wider ml-1">Trạng thái</label>
                  <select
                    value={projectForm.status}
                    onChange={(e) => setProjectForm({ ...projectForm, status: e.target.value })}
                    className="bg-slate-950/50 border border-slate-700/50 text-white px-4 py-3 rounded-xl focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all text-sm appearance-none"
                  >
                    <option value="Active">Đang hoạt động (Active)</option>
                    <option value="Maintenance">Bảo trì (Maintenance)</option>
                    <option value="Archived">Lưu trữ (Archived)</option>
                  </select>
                </div>

                <div className="flex flex-col gap-1.5 md:col-span-2">
                  <label className="text-xs font-bold text-slate-400 uppercase tracking-wider ml-1">Đường dẫn Website (Project URL)</label>
                  <input
                    type="url"
                    placeholder="https://example.com"
                    value={projectForm.projectUrl}
                    onChange={(e) => setProjectForm({ ...projectForm, projectUrl: e.target.value })}
                    className="bg-slate-950/50 border border-slate-700/50 text-white px-4 py-3 rounded-xl focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all text-sm placeholder:text-slate-600"
                  />
                </div>

                <div className="flex flex-col gap-1.5 md:col-span-2">
                  <label className="text-xs font-bold text-slate-400 uppercase tracking-wider ml-1">Đường dẫn Mã nguồn (Repository)</label>
                  <input
                    type="url"
                    placeholder="https://github.com/org/repo"
                    value={projectForm.repositoryUrl}
                    onChange={(e) => setProjectForm({ ...projectForm, repositoryUrl: e.target.value })}
                    className="bg-slate-950/50 border border-slate-700/50 text-white px-4 py-3 rounded-xl focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all text-sm placeholder:text-slate-600"
                  />
                </div>

                <div className="flex flex-col gap-1.5 md:col-span-2">
                  <label className="text-xs font-bold text-slate-400 uppercase tracking-wider ml-1">Mô tả chi tiết</label>
                  <textarea
                    placeholder="Mô tả về chức năng hoặc vai trò của dự án..."
                    value={projectForm.description}
                    onChange={(e) => setProjectForm({ ...projectForm, description: e.target.value })}
                    className="bg-slate-950/50 border border-slate-700/50 text-white px-4 py-3 rounded-xl focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all text-sm placeholder:text-slate-600 min-h-[80px]"
                  />
                </div>
              </div>
              <div className="flex justify-end gap-2 mt-4">
                <button
                  type="button"
                  className="px-4 py-2 text-sm text-slate-400 hover:text-white"
                  onClick={() => setShowModal(false)}
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-indigo-600 text-white rounded hover:bg-indigo-500"
                >
                  {isEditMode ? "Cập nhật" : "Tạo"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
