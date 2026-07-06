import { useEffect, useState, type FC } from "react";
import { Briefcase01, Building07, Clock, Users01 } from "@untitledui/icons";
import { TableCard } from "@/components/application/table/table";
import { ErrorBanner } from "@/components/banner";
import { LeaveStatusBadge } from "@/components/status-badge";
import { PageHeader } from "@/components/page-header";
import { dashboardApi } from "@/lib/api";
import { LEAVE_STATUSES, type DashboardStats } from "@/lib/types";

const StatCard = ({ label, value, icon: Icon }: { label: string; value: number; icon: FC<{ className?: string }> }) => (
    <div className="flex items-center gap-4 rounded-xl bg-primary p-5 shadow-xs ring-1 ring-secondary">
        <div className="flex size-11 shrink-0 items-center justify-center rounded-lg bg-brand-secondary text-fg-brand-primary">
            <Icon className="size-6" />
        </div>
        <div>
            <p className="text-sm text-tertiary">{label}</p>
            <p className="text-2xl font-semibold text-primary">{value}</p>
        </div>
    </div>
);

export const Dashboard = () => {
    const [stats, setStats] = useState<DashboardStats | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let active = true;
        dashboardApi
            .stats()
            .then((data) => active && setStats(data))
            .catch(() => active && setError("Failed to load dashboard data."));
        return () => {
            active = false;
        };
    }, []);

    const totalLeave = stats ? Object.values(stats.leaveByStatus).reduce((a, b) => a + b, 0) : 0;

    return (
        <div className="flex flex-col gap-6">
            <PageHeader title="Dashboard" subtitle="An overview of your organization." />

            {error && <ErrorBanner message={error} />}

            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
                <StatCard label="Departments" value={stats?.departmentCount ?? 0} icon={Building07} />
                <StatCard label="Employees" value={stats?.employeeCount ?? 0} icon={Users01} />
                <StatCard label="Positions" value={stats?.positionCount ?? 0} icon={Briefcase01} />
                <StatCard label="Pending leave" value={stats?.pendingLeaveCount ?? 0} icon={Clock} />
            </div>

            <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
                <div className="lg:col-span-2">
                    <TableCard.Root>
                        <TableCard.Header title="Recent employees" description="The latest people added to your organization." />
                        <div className="overflow-x-auto">
                            <table className="w-full text-left text-sm">
                                <thead className="border-b border-secondary bg-secondary">
                                    <tr className="text-xs font-semibold text-quaternary">
                                        <th className="px-6 py-3">Name</th>
                                        <th className="px-6 py-3">Department</th>
                                        <th className="px-6 py-3">Position</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {!stats || stats.recentEmployees.length === 0 ? (
                                        <tr>
                                            <td className="px-6 py-8 text-center text-tertiary" colSpan={3}>
                                                {stats ? "No employees yet." : "Loading…"}
                                            </td>
                                        </tr>
                                    ) : (
                                        stats.recentEmployees.map((e) => (
                                            <tr key={e.id} className="border-b border-secondary last:border-0">
                                                <td className="px-6 py-4 font-medium text-primary">
                                                    {e.firstName} {e.lastName}
                                                </td>
                                                <td className="px-6 py-4 text-tertiary">{e.departmentName}</td>
                                                <td className="px-6 py-4 text-tertiary">{e.positionTitle}</td>
                                            </tr>
                                        ))
                                    )}
                                </tbody>
                            </table>
                        </div>
                    </TableCard.Root>
                </div>

                <div className="rounded-xl bg-primary p-5 shadow-xs ring-1 ring-secondary">
                    <h2 className="text-md font-semibold text-primary">Leave requests</h2>
                    <p className="mt-0.5 text-sm text-tertiary">{totalLeave} total</p>
                    <div className="mt-4 flex flex-col gap-3">
                        {LEAVE_STATUSES.map((status) => (
                            <div key={status} className="flex items-center justify-between">
                                <LeaveStatusBadge status={status} />
                                <span className="text-sm font-medium text-secondary">{stats?.leaveByStatus[status] ?? 0}</span>
                            </div>
                        ))}
                    </div>
                </div>
            </div>
        </div>
    );
};
