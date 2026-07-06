import { createContext, useCallback, useContext, useRef, useState, type PropsWithChildren } from "react";
import { CheckCircle, AlertCircle, XClose } from "@untitledui/icons";
import { cx } from "@/utils/cx";

type ToastType = "success" | "error";

interface Toast {
    id: number;
    type: ToastType;
    message: string;
}

interface ToastContextValue {
    success: (message: string) => void;
    error: (message: string) => void;
}

const ToastContext = createContext<ToastContextValue | null>(null);

export const ToastProvider = ({ children }: PropsWithChildren) => {
    const [toasts, setToasts] = useState<Toast[]>([]);
    const nextId = useRef(1);

    const dismiss = useCallback((id: number) => {
        setToasts((prev) => prev.filter((t) => t.id !== id));
    }, []);

    const push = useCallback(
        (type: ToastType, message: string) => {
            const id = nextId.current++;
            setToasts((prev) => [...prev, { id, type, message }]);
            setTimeout(() => dismiss(id), 4500);
        },
        [dismiss],
    );

    const value: ToastContextValue = {
        success: (message) => push("success", message),
        error: (message) => push("error", message),
    };

    return (
        <ToastContext.Provider value={value}>
            {children}

            <div className="pointer-events-none fixed top-4 right-4 z-[100] flex w-full max-w-sm flex-col gap-3">
                {toasts.map((toast) => {
                    const Icon = toast.type === "success" ? CheckCircle : AlertCircle;
                    return (
                        <div
                            key={toast.id}
                            className="pointer-events-auto flex items-start gap-3 rounded-xl bg-primary p-4 shadow-lg ring-1 ring-secondary duration-200 animate-in fade-in slide-in-from-top-2"
                        >
                            <Icon
                                className={cx(
                                    "mt-0.5 size-5 shrink-0",
                                    toast.type === "success" ? "text-fg-success-primary" : "text-fg-error-primary",
                                )}
                            />
                            <p className="flex-1 text-sm text-secondary">{toast.message}</p>
                            <button
                                type="button"
                                onClick={() => dismiss(toast.id)}
                                className="text-fg-quaternary transition hover:text-fg-quaternary_hover"
                                aria-label="Dismiss"
                            >
                                <XClose className="size-4" />
                            </button>
                        </div>
                    );
                })}
            </div>
        </ToastContext.Provider>
    );
};

export const useToast = () => {
    const context = useContext(ToastContext);
    if (!context) {
        throw new Error("useToast must be used within a ToastProvider");
    }
    return context;
};
