"use client";
import React, { useEffect } from "react";
import { useMonitor } from "../context/MonitorContext";
import { useRouter } from "next/navigation";

export default function AuthPage() {
  const router = useRouter();
  const {
    token,
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
    handleAuth,
    handleVerifyOtp,
  } = useMonitor();

  useEffect(() => {
    if (token) {
      router.push("/");
    }
  }, [token, router]);

  return (
    <div className="flex-1 min-h-screen flex items-center justify-center py-12 relative overflow-hidden bg-slate-950">
      {/* Background Decorative Elements */}
      <div className="absolute top-[-10%] left-[-10%] w-[40%] h-[40%] bg-indigo-600/20 rounded-full blur-[120px] pointer-events-none"></div>
      <div className="absolute bottom-[-10%] right-[-10%] w-[40%] h-[40%] bg-violet-600/20 rounded-full blur-[120px] pointer-events-none"></div>

      <div className="w-full max-w-md bg-slate-900/50 border border-slate-700/50 rounded-[2rem] p-10 backdrop-blur-2xl shadow-[0_0_40px_rgba(79,70,229,0.1)] relative z-10 flex flex-col gap-8">
        
        {/* Header Section */}
        <div className="flex flex-col items-center justify-center text-center gap-2">
          <div className="w-16 h-16 rounded-2xl bg-gradient-to-br from-indigo-500 to-violet-600 flex items-center justify-center shadow-lg shadow-indigo-500/30 mb-2">
            <svg className="w-8 h-8 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 002 2h2a2 2 0 002-2z" />
            </svg>
          </div>
          <h2 className="text-3xl font-black tracking-tight bg-gradient-to-r from-white to-slate-400 bg-clip-text text-transparent">
            {isOtpPending ? "Xác thực OTP" : isLogin ? "Đăng nhập" : "Tạo tài khoản"}
          </h2>
          <p className="text-sm text-slate-400 font-medium">
            {isOtpPending
              ? `Nhập mã OTP để kích hoạt \`${authUsername}\``
              : isLogin
              ? "Hệ thống giám sát dịch vụ & Website"
              : "Đăng ký tài khoản Admin/Viewer"}
          </p>
        </div>

        {isOtpPending ? (
          <form onSubmit={handleVerifyOtp} className="flex flex-col gap-6">
            <div className="flex flex-col gap-2">
              <input
                type="text"
                required
                value={otpCode}
                onChange={(e) => setOtpCode(e.target.value)}
                maxLength={6}
                className="bg-slate-950/50 border border-slate-700/50 rounded-2xl px-4 py-4 text-center text-2xl font-black tracking-[0.5em] text-indigo-400 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all placeholder:text-slate-600 placeholder:tracking-normal"
                placeholder="Nhập 6 số"
              />
            </div>
            <button
              type="submit"
              className="w-full bg-gradient-to-r from-indigo-500 to-violet-600 hover:from-indigo-400 hover:to-violet-500 text-white py-4 rounded-2xl text-sm font-bold shadow-lg shadow-indigo-500/25 transition-all transform hover:-translate-y-0.5 active:translate-y-0"
            >
              Xác nhận mã OTP
            </button>
            <button
              type="button"
              onClick={() => setIsOtpPending(false)}
              className="text-sm text-slate-500 hover:text-slate-300 transition-colors font-medium text-center"
            >
              Quay lại Đăng ký
            </button>
          </form>
        ) : (
          <form onSubmit={handleAuth} className="flex flex-col gap-6">
            {/* Tabs */}
            <div className="flex bg-slate-950/50 p-1.5 rounded-2xl border border-slate-800/80 backdrop-blur-sm">
              <button
                type="button"
                onClick={() => { setIsLogin(true); setIsOtpPending(false); }}
                className={`flex-1 py-2.5 text-sm font-bold rounded-xl transition-all ${
                  isLogin ? "bg-slate-800/80 text-white shadow-sm" : "text-slate-400 hover:text-slate-200"
                }`}
              >
                Đăng nhập
              </button>
              <button
                type="button"
                onClick={() => { setIsLogin(false); setIsOtpPending(false); }}
                className={`flex-1 py-2.5 text-sm font-bold rounded-xl transition-all ${
                  !isLogin ? "bg-slate-800/80 text-white shadow-sm" : "text-slate-400 hover:text-slate-200"
                }`}
              >
                Đăng ký
              </button>
            </div>

            <div className="flex flex-col gap-4">
              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-slate-400 uppercase tracking-wider ml-1">Tài khoản</label>
                <input
                  type="text"
                  required
                  value={authUsername}
                  onChange={(e) => setAuthUsername(e.target.value)}
                  className="bg-slate-950/50 border border-slate-700/50 rounded-2xl px-4 py-3.5 text-sm text-white focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all placeholder:text-slate-600"
                  placeholder="Nhập tên đăng nhập..."
                />
              </div>

              {!isLogin && (
                <div className="flex flex-col gap-1.5">
                  <label className="text-xs font-bold text-slate-400 uppercase tracking-wider ml-1">Email</label>
                  <input
                    type="email"
                    required
                    value={authEmail}
                    onChange={(e) => setAuthEmail(e.target.value)}
                    className="bg-slate-950/50 border border-slate-700/50 rounded-2xl px-4 py-3.5 text-sm text-white focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all placeholder:text-slate-600"
                    placeholder="name@company.com"
                  />
                </div>
              )}

              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-slate-400 uppercase tracking-wider ml-1">Mật khẩu</label>
                <input
                  type="password"
                  required
                  value={authPassword}
                  onChange={(e) => setAuthPassword(e.target.value)}
                  className="bg-slate-950/50 border border-slate-700/50 rounded-2xl px-4 py-3.5 text-sm text-white focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-all placeholder:text-slate-600"
                  placeholder="••••••••••••"
                />
              </div>
            </div>

            <div className="flex flex-col gap-4 mt-2">
              <button
                type="submit"
                className="w-full bg-gradient-to-r from-indigo-500 to-violet-600 hover:from-indigo-400 hover:to-violet-500 text-white py-4 rounded-2xl text-sm font-bold shadow-lg shadow-indigo-500/25 transition-all transform hover:-translate-y-0.5 active:translate-y-0"
              >
                {isLogin ? "Truy cập hệ thống" : "Tạo tài khoản mới"}
              </button>

              {!isLogin && (
                <button
                  type="button"
                  onClick={() => setIsOtpPending(true)}
                  className="text-xs text-indigo-400 hover:text-indigo-300 transition-colors font-medium text-center"
                >
                  Đã có tài khoản nhưng chưa kích hoạt OTP?
                </button>
              )}
            </div>
          </form>
        )}
      </div>
    </div>
  );
}
