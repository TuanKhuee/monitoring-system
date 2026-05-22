 "use client";

import { useMonitor } from "../context/MonitorContext";

export default function ToastAlert() {
  const { toast } = useMonitor();
  if (!toast) return null;
  return (
    <div
      className={`fixed top-5 right-5 z-50 flex items-center gap-3 px-5 py-4 rounded-xl shadow-2xl backdrop-blur-md border animate-bounce ${
        toast.type === "success"
          ? "bg-emerald-950/80 border-emerald-500 text-emerald-200"
          : "bg-rose-950/80 border-rose-500 text-rose-200"
      }`}
    >
      {toast.type === "success" ? (
        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M5 13l4 4L19 7" />
        </svg>
      ) : (
        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M6 18L18 6M6 6l12 12" />
        </svg>
      )}
      <span className="text-sm font-semibold">{toast.message}</span>
    </div>
  );
}
