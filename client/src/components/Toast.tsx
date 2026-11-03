import React from 'react';
import { CheckCircle2, AlertTriangle, X } from 'lucide-react';

export interface ToastData {
  title: string;
  message: string;
  isSuccess: boolean;
}

interface ToastProps {
  toast: ToastData | null;
  onClose: () => void;
}

export const Toast: React.FC<ToastProps> = ({ toast, onClose }) => {
  if (!toast) return null;

  return (
    <div className="fixed top-24 right-5 z-50 animate-in slide-in-from-top-4 duration-300 pointer-events-auto">
      <div className="px-5 py-3.5 rounded-2xl bg-slate-900 text-white shadow-2xl border border-slate-700 flex items-center gap-3 max-w-sm">
        <div
          className={`w-8 h-8 rounded-xl flex items-center justify-center shrink-0 ${
            toast.isSuccess ? 'bg-emerald-500/20 text-emerald-400' : 'bg-red-500/20 text-red-400'
          }`}
        >
          {toast.isSuccess ? <CheckCircle2 className="w-4 h-4" /> : <AlertTriangle className="w-4 h-4" />}
        </div>
        <div className="flex-1">
          <p className="text-xs font-bold text-white">{toast.title}</p>
          <p className="text-[11px] text-slate-300 mt-0.5 leading-snug">{toast.message}</p>
        </div>
        <button
          onClick={onClose}
          className="text-slate-400 hover:text-white transition-colors cursor-pointer"
        >
          <X className="w-3.5 h-3.5" />
        </button>
      </div>
    </div>
  );
};
