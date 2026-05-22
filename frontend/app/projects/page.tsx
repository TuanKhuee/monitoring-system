"use client";

import Header from "../components/Header";
import ToastAlert from "../components/ToastAlert";

export default function ProjectsPage() {
  return (
    <>
      <Header />
      <main className="flex-1 flex items-center justify-center p-6">
        <h2 className="text-2xl font-bold text-white">Projects Page (TODO: implement project management UI)</h2>
      </main>
      <ToastAlert />
    </>
  );
}
