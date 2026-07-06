import { useEffect, useState, type FC } from "react";
import { Building07, Briefcase01, Users01 } from "@untitledui/icons";
import { TableCard } from "@/components/application/table/table";
import { departmentsApi, employeesApi, positionsApi } from "@/lib/api";
import type { Employee } from "@/lib/types";

interface Stats {
    departments: number;
    employees: number;
    positions: number;
}

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
    const [stats, setStats] = useState<Stats | null>(null);
    const [recent, setRecent] = useState<Employee[]>([]);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let active = true;
        (async () => {
            try {
                const [departments, employees, positions] = await Promise.all([
                    departmentsApi.list(),
                    employeesApi.list(),
                    positionsApi.list(),
                ]);
                if (!active) return;
                setStats({ departments: departments.length, employees: employees.length, positions: positions.length });
                setRecent(employees.slice(-5).reverse());
            } catch {
                if (active) setError("Failed to load dashboard data.");
            }
        })();
        return () => {
            active = false;
        };
    }, []);

    return (
        <div className="flex flex-col gap-6">
            <div>
                <h1 className="text-2xl font-semibold text-primary">Dashboard</h1>
                <p className="mt-1 text-sm text-tertiary">An overview of your organization.</p>
            </div>

            {error && (
                <div className="rounded-lg bg-error-primary px-3.5 py-2.5 text-sm text-error-primary ring-1 ring-error_subtle ring-inset">
                    {error}
                </div>
            )}

            <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
                <StatCard label="Departments" value={stats?.departments ?? 0} icon={Building07} />
                <StatCard label="Employees" value={stats?.employees ?? 0} icon={Users01} />
                <StatCard label="Positions" value={stats?.positions ?? 0} icon={Briefcase01} />
            </div>

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
                            {recent.length === 0 ? (
                                <tr>
                                    <td className="px-6 py-8 text-center text-tertiary" colSpan={3}>
                                        No employees yet.
                                    </td>
                                </tr>
                            ) : (
                                recent.map((e) => (
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
    );
};
