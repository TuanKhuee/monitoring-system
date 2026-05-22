"use client";
import React, { createContext, useContext, useState, useEffect, ReactNode } from "react";

// Types
export interface User {
  username: string;
  role: string;
}

export interface Project {
  id: string;
  name: string;
  description: string;
  projectCode: string;
  projectUrl: string;
  repositoryUrl: string;
  status: string;
  createdAt: string;
}

export interface Service {
  id: string;
  projectId: string;
  serviceName: string;
  ip: string;
  port: number;
  protocol: string;
  healthEndpoint?: string;
  intervalSeconds: number;
  isActive: boolean;
}

export interface MonitorLog {
  id: string;
  serviceId: string;
  status: number;
  responseTimeMs: number;
  statusCode: number;
  errorMassage: string;
  checkedAt: string;
}

export interface DashboardStats {
  totalServices: number;
  activeServices: number;
  onlineServices: number;
  offlineServices: number;
  overallUptime24h: number;
  serverTime: string;
}

interface MonitorContextProps {
  apiUrl: string;
  setApiUrl: (url: string) => void;
  token: string | null;
  setToken: (t: string | null) => void;
  user: User | null;
  setUser: (u: User | null) => void;
  projects: Project[];
  setProjects: (p: Project[]) => void;
  services: Service[];
  setServices: (s: Service[]) => void;
  selectedProject: Project | null;
  setSelectedProject: (p: Project | null) => void;
  stats: DashboardStats | null;
  setStats: (s: DashboardStats | null) => void;
  recentLogs: MonitorLog[];
  setRecentLogs: (l: MonitorLog[]) => void;
  toast: { message: string; type: "success" | "error" } | null;
  setToast: (t: { message: string; type: "success" | "error" } | null) => void;
  // Auth actions
  handleAuth: (e: React.FormEvent) => Promise<void>;
  handleVerifyOtp: (e: React.FormEvent) => Promise<void>;
  handleLogout: () => void;
  // Data fetches
  fetchDashboardStats: () => Promise<void>;
  fetchRecentLogs: () => Promise<void>;
  fetchProjects: () => Promise<void>;
  fetchServices: (projectId: string) => Promise<void>;
  // CRUD actions
  handleCreateProject: (e: React.FormEvent) => Promise<void>;
  handleEditProject: (proj: Project) => void;
  handleDeleteProject: (id: string) => Promise<void>;
  handleCreateService: (e: React.FormEvent) => Promise<void>;
  handleUpdateService: (id: string, updatedService: Service) => Promise<void>;
  handleDeleteService: (id: string) => Promise<void>;
  // Misc
  handleSaveApiUrl: () => void;
  // UI Auth State
  isLogin: boolean;
  setIsLogin: (val: boolean) => void;
  authUsername: string;
  setAuthUsername: (val: string) => void;
  authEmail: string;
  setAuthEmail: (val: string) => void;
  authPassword: string;
  setAuthPassword: (val: string) => void;
  otpCode: string;
  setOtpCode: (val: string) => void;
  isOtpPending: boolean;
  setIsOtpPending: (val: boolean) => void;
  // UI Form State
  projectForm: any;
  setProjectForm: (val: any) => void;
  serviceForm: any;
  setServiceForm: (val: any) => void;
  // Tabs & Modals
  activeTab: string;
  setActiveTab: (val: string) => void;
  showProjectModal: boolean;
  setShowProjectModal: (val: boolean) => void;
  showServiceModal: boolean;
  setShowServiceModal: (val: boolean) => void;
}

const MonitorContext = createContext<MonitorContextProps | undefined>(undefined);

export const useMonitor = () => {
  const ctx = useContext(MonitorContext);
  if (!ctx) {
    throw new Error("useMonitor must be used within MonitorProvider");
  }
  return ctx;
};

export const MonitorProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  // API settings
  const [apiUrl, setApiUrl] = useState("http://localhost:5053");
  const [token, setToken] = useState<string | null>(null);
  const [user, setUser] = useState<User | null>(null);

  // Core data states
  const [projects, setProjects] = useState<Project[]>([]);
  const [services, setServices] = useState<Service[]>([]);
  const [selectedProject, setSelectedProject] = useState<Project | null>(null);
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [recentLogs, setRecentLogs] = useState<MonitorLog[]>([]);

  // Toast state
  const [toast, setToast] = useState<{ message: string; type: "success" | "error" } | null>(null);

  // Helpers
  const showToast = (message: string, type: "success" | "error" = "success") => {
    setToast({ message, type });
    setTimeout(() => setToast(null), 4000);
  };

  // Load token from localStorage on mount
  useEffect(() => {
    const savedToken = localStorage.getItem("monitor_token");
    const savedUser = localStorage.getItem("monitor_user");
    const savedApiUrl = localStorage.getItem("monitor_api_url");
    if (savedApiUrl) setApiUrl(savedApiUrl);
    if (savedToken) {
      setToken(savedToken);
      if (savedUser) {
        try {
          setUser(JSON.parse(savedUser));
        } catch {
          setUser(null);
        }
      }
    }
  }, []);

  // Periodic fetch when authenticated
  useEffect(() => {
    if (!token) return;
    fetchDashboardStats();
    fetchRecentLogs();
    fetchProjects();
    const interval = setInterval(() => {
      fetchDashboardStats();
      fetchRecentLogs();
    }, 30000);
    return () => clearInterval(interval);
  }, [token, apiUrl]);

  // Load services when selected project changes
  useEffect(() => {
    if (selectedProject) {
      fetchServices(selectedProject.id);
    } else {
      setServices([]);
    }
  }, [selectedProject, token, apiUrl]);

  // API Calls
  const fetchProjects = async () => {
    try {
      const res = await fetch(`${apiUrl}/api/Project`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (res.ok) {
        const data = await res.json();
        setProjects(data);
        if (data.length > 0 && !selectedProject) setSelectedProject(data[0]);
      }
    } catch (err) {
      console.error("Error fetching projects:", err);
    }
  };

  const fetchServices = async (projectId: string) => {
    try {
      const res = await fetch(`${apiUrl}/api/Service/Project/${projectId}`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (res.ok) {
        const data = await res.json();
        setServices(data);
      }
    } catch (err) {
      console.error("Error fetching services:", err);
    }
  };

  const fetchDashboardStats = async () => {
    try {
      const res = await fetch(`${apiUrl}/api/Dashboard/Stats`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (res.ok) {
        const data = await res.json();
        setStats(data);
      }
    } catch (err) {
      console.error("Error fetching dashboard stats:", err);
    }
  };

  const fetchRecentLogs = async () => {
    try {
      const res = await fetch(`${apiUrl}/api/Dashboard/RecentLogs?limit=15`, {
        headers: { Authorization: `Bearer ${token}` },
      });
      if (res.ok) {
        const data = await res.json();
        setRecentLogs(data);
      }
    } catch (err) {
      console.error("Error fetching recent logs:", err);
    }
  };

  // Auth actions
  const handleAuth = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (isLogin) {
        const res = await fetch(`${apiUrl}/api/Auth/Login`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ username: authUsername, password: authPassword }),
        });
        const data = await res.json();
        if (res.ok) {
          localStorage.setItem("monitor_token", data.token);
          const userData = { username: authUsername, role: data.role || "Viewer" };
          localStorage.setItem("monitor_user", JSON.stringify(userData));
          setToken(data.token);
          setUser(userData);
          showToast("Đăng nhập hệ thống thành công!", "success");
        } else {
          showToast(data.message || "Đăng nhập thất bại. Vui lòng thử lại.", "error");
        }
      } else {
        const res = await fetch(`${apiUrl}/api/Auth/Register`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ username: authUsername, email: authEmail, password: authPassword }),
        });
        if (res.ok) {
          showToast("Đăng ký thành công! Hãy nhập mã kích hoạt gửi tới bạn.", "success");
          setIsOtpPending(true);
        } else {
          const data = await res.json();
          showToast(data.message || "Đăng ký thất bại.", "error");
        }
      }
    } catch (err) {
      showToast("Lỗi kết nối tới máy chủ Backend.", "error");
    }
  };

  const handleVerifyOtp = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await fetch(`${apiUrl}/api/Auth/VerifyOtp`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username: authUsername, otpCode }),
      });
      if (res.ok) {
        showToast("Xác thực OTP thành công! Bây giờ bạn đã có thể Đăng nhập.", "success");
        setIsOtpPending(false);
        setIsLogin(true);
        setOtpCode("");
      } else {
        const data = await res.json();
        showToast(data.message || "Mã kích hoạt OTP không hợp lệ.", "error");
      }
    } catch (err) {
      showToast("Lỗi kết nối tới máy chủ Backend.", "error");
    }
  };

  const handleLogout = () => {
    localStorage.removeItem("monitor_token");
    localStorage.removeItem("monitor_user");
    setToken(null);
    setUser(null);
    setProjects([]);
    setServices([]);
    setSelectedProject(null);
    setStats(null);
    setRecentLogs([]);
    showToast("Đã đăng xuất khỏi hệ thống.", "success");
  };

  // Project CRUD
  const handleCreateProject = async (e: React.FormEvent) => {
    e.preventDefault();
    const isEdit = !!projectForm.id;
    const url = isEdit ? `${apiUrl}/api/Project/${projectForm.id}` : `${apiUrl}/api/Project`;
    const method = isEdit ? "PUT" : "POST";
    try {
      const res = await fetch(url, {
        method,
        headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
        body: JSON.stringify(projectForm),
      });
      if (res.ok) {
        showToast(isEdit ? "Cập nhật dự án thành công!" : "Tạo dự án mới thành công!");
        // Caller will close modal and reset form
        fetchProjects();
      } else {
        const data = await res.json();
        showToast(
          data.message ||
            (isEdit ? "Không thể cập nhật dự án. Bạn có phải Admin?" : "Không thể tạo dự án. Bạn có phải Admin?"),
          "error"
        );
      }
    } catch {
      showToast("Lỗi kết nối máy chủ.", "error");
    }
  };

  const handleEditProject = (proj: Project) => {
    // Intended to be used by UI component to populate form, not needed in context
    // No operation here
  };

  const handleDeleteProject = async (id: string) => {
    if (!confirm("Bạn có chắc chắn muốn xóa dự án này?")) return;
    try {
      const res = await fetch(`${apiUrl}/api/Project/${id}`, {
        method: "DELETE",
        headers: { Authorization: `Bearer ${token}` },
      });
      if (res.ok) {
        showToast("Xóa dự án thành công!");
        fetchProjects();
      } else {
        const data = await res.json();
        showToast(data.message || "Xóa dự án thất bại.", "error");
      }
    } catch {
      showToast("Lỗi kết nối máy chủ.", "error");
    }
  };

  // Service CRUD
  const handleCreateService = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedProject) return;
    try {
      const res = await fetch(`${apiUrl}/api/Service`, {
        method: "POST",
        headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
        body: JSON.stringify({
          ...serviceForm,
          projectId: selectedProject.id,
          port: Number(serviceForm.port),
          intervalSeconds: Number(serviceForm.intervalSeconds),
        }),
      });
      if (res.ok) {
        showToast("Thêm dịch vụ cần giám sát thành công!");
        fetchServices(selectedProject.id);
        fetchDashboardStats();
      } else {
        const data = await res.json();
        showToast(data.message || "Lỗi tạo dịch vụ. Hãy kiểm tra lại quyền Admin.", "error");
      }
    } catch {
      showToast("Lỗi kết nối máy chủ.", "error");
    }
  };

  const handleUpdateService = async (id: string, updatedService: Service) => {
    try {
      const res = await fetch(`${apiUrl}/api/Service/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json", Authorization: `Bearer ${token}` },
        body: JSON.stringify(updatedService),
      });
      if (res.ok) {
        showToast("Cập nhật dịch vụ thành công!");
        if (selectedProject) fetchServices(selectedProject.id);
        fetchDashboardStats();
      } else {
        const data = await res.json();
        showToast(data.message || "Cập nhật dịch vụ thất bại.", "error");
      }
    } catch {
      showToast("Lỗi kết nối máy chủ.", "error");
    }
  };

  const handleDeleteService = async (id: string) => {
    if (!confirm("Bạn có chắc chắn muốn xóa dịch vụ giám sát này?")) return;
    try {
      const res = await fetch(`${apiUrl}/api/Service/${id}`, {
        method: "DELETE",
        headers: { Authorization: `Bearer ${token}` },
      });
      if (res.ok) {
        showToast("Xóa dịch vụ thành công!");
        if (selectedProject) fetchServices(selectedProject.id);
        fetchDashboardStats();
      } else {
        showToast("Không thể xóa. Cần vai trò Admin.", "error");
      }
    } catch {
      showToast("Lỗi kết nối.", "error");
    }
  };

  const handleSaveApiUrl = () => {
    localStorage.setItem("monitor_api_url", apiUrl);
    showToast(`Đã lưu URL Backend API: ${apiUrl}`);
  };

  // UI state for auth forms (kept locally in context for convenience)
  const [isLogin, setIsLogin] = useState(true);
  const [authUsername, setAuthUsername] = useState("");
  const [authEmail, setAuthEmail] = useState("");
  const [authPassword, setAuthPassword] = useState("");
  const [otpCode, setOtpCode] = useState("");
  const [isOtpPending, setIsOtpPending] = useState(false);

  // UI state for project/service forms (used by modal components)
  const [projectForm, setProjectForm] = useState({
    id: "",
    name: "",
    description: "",
    projectCode: "",
    projectUrl: "",
    repositoryUrl: "",
    status: "Active",
  });

  const [serviceForm, setServiceForm] = useState({
    serviceName: "",
    ip: "",
    port: 80,
    protocol: "Http",
    healthEndpoint: "/",
    intervalSeconds: 60,
    isActive: true,
  });

  const [activeTab, setActiveTab] = useState("dashboard");
  const [showProjectModal, setShowProjectModal] = useState(false);
  const [showServiceModal, setShowServiceModal] = useState(false);

  return (
    <MonitorContext.Provider
      value={{
        apiUrl,
        setApiUrl,
        token,
        setToken,
        user,
        setUser,
        projects,
        setProjects,
        services,
        setServices,
        selectedProject,
        setSelectedProject,
        stats,
        setStats,
        recentLogs,
        setRecentLogs,
        toast,
        setToast,
        handleAuth,
        handleVerifyOtp,
        handleLogout,
        fetchDashboardStats,
        fetchRecentLogs,
        fetchProjects,
        fetchServices,
        handleCreateProject,
        handleEditProject,
        handleDeleteProject,
        handleCreateService,
        handleUpdateService,
        handleDeleteService,
        handleSaveApiUrl,
        isLogin,
        setIsLogin,
        authUsername,
        setAuthUsername,
        authEmail,
        setAuthEmail,
        authPassword,
        setAuthPassword,
        otpCode,
        setOtpCode,
        isOtpPending,
        setIsOtpPending,
        projectForm,
        setProjectForm,
        serviceForm,
        setServiceForm,
        activeTab,
        setActiveTab,
        showProjectModal,
        setShowProjectModal,
        showServiceModal,
        setShowServiceModal,
      }}
    >
      {children}
    </MonitorContext.Provider>
  );
};
