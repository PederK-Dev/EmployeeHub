import { AlertCircle } from "@untitledui/icons";

interface BannerProps {
    message: string;
}

/** Inline error banner used at the top of pages and forms. */
export const ErrorBanner = ({ message }: BannerProps) => (
    <div className="flex items-center gap-2 rounded-lg bg-error-primary px-3.5 py-2.5 text-sm text-error-primary ring-1 ring-error_subtle ring-inset">
        <AlertCircle className="size-4 shrink-0" />
        <span>{message}</span>
    </div>
);
