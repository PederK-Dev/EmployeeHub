import type { ReactNode } from "react";

interface PageHeaderProps {
    title: string;
    subtitle?: string;
    action?: ReactNode;
}

export const PageHeader = ({ title, subtitle, action }: PageHeaderProps) => (
    <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
            <h1 className="text-2xl font-semibold text-primary">{title}</h1>
            {subtitle && <p className="mt-1 text-sm text-tertiary">{subtitle}</p>}
        </div>
        {action}
    </div>
);
