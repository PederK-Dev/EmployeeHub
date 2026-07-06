import { Building07, Home03, LogOut01, Users01 } from "@untitledui/icons";
import { NavLink, Outlet } from "react-router";
import { Avatar } from "@/components/base/avatar/avatar";
import { Button } from "@/components/base/buttons/button";
import { useAuth } from "@/providers/auth-provider";
import { cx } from "@/utils/cx";

const navItems = [
    { to: "/", label: "Dashboard", icon: Home03, end: true },
    { to: "/departments", label: "Departments", icon: Building07, end: false },
    { to: "/employees", label: "Employees", icon: Users01, end: false },
];

export const AppLayout = () => {
    const { user, logout } = useAuth();

    return (
        <div className="flex min-h-dvh bg-secondary">
            <aside className="flex w-64 shrink-0 flex-col border-r border-secondary bg-primary px-4 py-6">
                <div className="mb-8 flex items-center gap-2 px-2">
                    <div className="flex size-8 items-center justify-center rounded-lg bg-brand-solid text-white">
                        <Building07 className="size-5" />
                    </div>
                    <span className="text-lg font-semibold text-primary">EmployeeHub</span>
                </div>

                <nav className="flex flex-1 flex-col gap-1">
                    {navItems.map(({ to, label, icon: Icon, end }) => (
                        <NavLink
                            key={to}
                            to={to}
                            end={end}
                            className={({ isActive }) =>
                                cx(
                                    "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition duration-100 ease-linear",
                                    isActive
                                        ? "bg-secondary text-primary"
                                        : "text-tertiary hover:bg-primary_hover hover:text-secondary_hover",
                                )
                            }
                        >
                            <Icon className="size-5 shrink-0" aria-hidden="true" />
                            {label}
                        </NavLink>
                    ))}
                </nav>

                <div className="mt-4 flex flex-col gap-3 border-t border-secondary pt-4">
                    <div className="flex items-center gap-3 px-2">
                        <Avatar size="sm" initials={user?.email?.slice(0, 2).toUpperCase() ?? "?"} alt={user?.email ?? "User"} />
                        <div className="min-w-0 flex-1">
                            <p className="truncate text-sm font-medium text-primary">{user?.email}</p>
                            <p className="truncate text-xs text-tertiary">{user?.role}</p>
                        </div>
                    </div>
                    <Button size="sm" color="secondary" iconLeading={LogOut01} onClick={logout}>
                        Sign out
                    </Button>
                </div>
            </aside>

            <main className="flex-1 overflow-x-auto">
                <div className="mx-auto max-w-6xl px-6 py-8">
                    <Outlet />
                </div>
            </main>
        </div>
    );
};
