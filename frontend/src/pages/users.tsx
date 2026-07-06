import { useEffect, useMemo, useState, type FormEvent } from "react";
import { Trash01, UserPlus01 } from "@untitledui/icons";
import { Badge } from "@/components/base/badges/badges";
import { Button } from "@/components/base/buttons/button";
import { Input } from "@/components/base/input/input";
import { NativeSelect } from "@/components/base/select/select-native";
import { ErrorBanner } from "@/components/banner";
import { ConfirmModal } from "@/components/confirm-modal";
import { DataTable, type Column } from "@/components/data-table";
import { ModalShell } from "@/components/modal-shell";
import { PageHeader } from "@/components/page-header";
import { ApiError, employeesApi, usersApi } from "@/lib/api";
import { USER_ROLES, type Employee, type User, type UserRole } from "@/lib/types";
import { useAuth } from "@/providers/auth-provider";
import { useToast } from "@/providers/toast-provider";

interface FormState {
    email: string;
    password: string;
    role: UserRole;
    employeeId: string;
}

const emptyForm: FormState = { email: "", password: "", role: "Employee", employeeId: "" };

export const Users = () => {
    const toast = useToast();
    const { user: currentUser } = useAuth();
    const [users, setUsers] = useState<User[]>([]);
    const [employees, setEmployees] = useState<Employee[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [query, setQuery] = useState("");

    const [isFormOpen, setIsFormOpen] = useState(false);
    const [form, setForm] = useState<FormState>(emptyForm);
    const [formError, setFormError] = useState<string | null>(null);
    const [isSaving, setIsSaving] = useState(false);

    const [deleteTarget, setDeleteTarget] = useState<User | null>(null);
    const [isDeleting, setIsDeleting] = useState(false);

    const load = async () => {
        setIsLoading(true);
        try {
            setUsers(await usersApi.list());
            setError(null);
        } catch {
            setError("Failed to load users.");
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        load();
        employeesApi
            .list()
            .then(setEmployees)
            .catch(() => setError("Failed to load employees."));
    }, []);

    const filtered = useMemo(() => {
        const q = query.trim().toLowerCase();
        if (!q) return users;
        return users.filter((u) => `${u.email} ${u.role}`.toLowerCase().includes(q));
    }, [users, query]);

    const setField = (key: keyof FormState, value: string) => setForm((prev) => ({ ...prev, [key]: value }));

    const openCreate = () => {
        setForm(emptyForm);
        setFormError(null);
        setIsFormOpen(true);
    };

    const handleSubmit = async (event: FormEvent) => {
        event.preventDefault();
        setFormError(null);
        setIsSaving(true);
        try {
            await usersApi.create({
                email: form.email,
                password: form.password,
                role: form.role,
                employeeId: form.employeeId === "" ? null : Number(form.employeeId),
            });
            toast.success("User created.");
            setIsFormOpen(false);
            await load();
        } catch (err) {
            setFormError(err instanceof ApiError ? err.message : "Something went wrong.");
        } finally {
            setIsSaving(false);
        }
    };

    const confirmDelete = async () => {
        if (!deleteTarget) return;
        setIsDeleting(true);
        try {
            await usersApi.remove(deleteTarget.id);
            toast.success("User deleted.");
            setDeleteTarget(null);
            await load();
        } catch (err) {
            toast.error(err instanceof ApiError ? err.message : "Failed to delete user.");
            setDeleteTarget(null);
        } finally {
            setIsDeleting(false);
        }
    };

    const employeeName = (id: number | null) => {
        if (id === null) return "—";
        const match = employees.find((e) => e.id === id);
        return match ? `${match.firstName} ${match.lastName}` : `#${id}`;
    };

    const roleColor: Record<UserRole, "brand" | "gray" | "success"> = {
        Admin: "brand",
        Manager: "success",
        Employee: "gray",
    };

    // Only employees not already linked to a user can be attached to a new account.
    const linkedEmployeeIds = new Set(users.map((u) => u.employeeId).filter((id): id is number => id !== null));
    const employeeOptions = [
        { label: "No linked employee", value: "" },
        ...employees
            .filter((e) => !linkedEmployeeIds.has(e.id))
            .map((e) => ({ label: `${e.firstName} ${e.lastName}`, value: String(e.id) })),
    ];

    const columns: Column<User>[] = [
        { header: "Email", render: (u) => u.email, cellClassName: "font-medium text-primary" },
        {
            header: "Role",
            render: (u) => (
                <Badge size="sm" type="pill-color" color={roleColor[u.role]}>
                    {u.role}
                </Badge>
            ),
        },
        { header: "Linked employee", render: (u) => employeeName(u.employeeId) },
        {
            header: "Actions",
            align: "right",
            render: (u) => (
                <div className="flex justify-end">
                    <Button
                        size="sm"
                        color="tertiary-destructive"
                        iconLeading={Trash01}
                        isDisabled={u.id === currentUser?.id}
                        title={u.id === currentUser?.id ? "You cannot delete your own account" : undefined}
                        onClick={() => setDeleteTarget(u)}
                    >
                        Delete
                    </Button>
                </div>
            ),
        },
    ];

    return (
        <div className="flex flex-col gap-6">
            <PageHeader
                title="Users"
                subtitle="Manage accounts that can sign in to EmployeeHub."
                action={
                    <Button iconLeading={UserPlus01} onClick={openCreate}>
                        Add user
                    </Button>
                }
            />

            {error && <ErrorBanner message={error} />}

            <DataTable
                title="All users"
                columns={columns}
                rows={filtered}
                getKey={(u) => u.id}
                isLoading={isLoading}
                emptyMessage={query ? "No users match your search." : "No users yet."}
                search={{ value: query, onChange: setQuery, placeholder: "Search users" }}
            />

            <ModalShell isOpen={isFormOpen} onOpenChange={setIsFormOpen} title="Add user">
                <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                    {formError && <ErrorBanner message={formError} />}
                    <Input label="Email" type="email" value={form.email} onChange={(v) => setField("email", v)} isRequired />
                    <Input
                        label="Password"
                        type="password"
                        value={form.password}
                        onChange={(v) => setField("password", v)}
                        hint="At least 8 characters."
                        isRequired
                    />
                    <NativeSelect
                        label="Role"
                        options={USER_ROLES.map((r) => ({ label: r, value: r }))}
                        value={form.role}
                        onChange={(e) => setField("role", e.target.value)}
                    />
                    <NativeSelect
                        label="Linked employee"
                        options={employeeOptions}
                        value={form.employeeId}
                        onChange={(e) => setField("employeeId", e.target.value)}
                    />
                    <div className="mt-2 flex justify-end gap-3">
                        <Button color="secondary" onClick={() => setIsFormOpen(false)} type="button">
                            Cancel
                        </Button>
                        <Button type="submit" isLoading={isSaving}>
                            Create user
                        </Button>
                    </div>
                </form>
            </ModalShell>

            <ConfirmModal
                isOpen={deleteTarget !== null}
                title="Delete user"
                description={`Are you sure you want to delete ${deleteTarget?.email}? This cannot be undone.`}
                isConfirming={isDeleting}
                onConfirm={confirmDelete}
                onClose={() => setDeleteTarget(null)}
            />
        </div>
    );
};
