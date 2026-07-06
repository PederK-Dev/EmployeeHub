import { useEffect, useMemo, useState, type FormEvent } from "react";
import { Edit01, Plus, Trash01 } from "@untitledui/icons";
import { Button } from "@/components/base/buttons/button";
import { Input } from "@/components/base/input/input";
import { NativeSelect } from "@/components/base/select/select-native";
import { ErrorBanner } from "@/components/banner";
import { ConfirmModal } from "@/components/confirm-modal";
import { DataTable, type Column } from "@/components/data-table";
import { ModalShell } from "@/components/modal-shell";
import { PageHeader } from "@/components/page-header";
import { ApiError, departmentsApi, employeesApi, positionsApi } from "@/lib/api";
import type { Department, Employee, Position } from "@/lib/types";
import { useToast } from "@/providers/toast-provider";

interface FormState {
    firstName: string;
    lastName: string;
    email: string;
    hireDate: string;
    departmentId: string;
    positionId: string;
}

const emptyForm: FormState = {
    firstName: "",
    lastName: "",
    email: "",
    hireDate: "",
    departmentId: "",
    positionId: "",
};

export const Employees = () => {
    const toast = useToast();
    const [employees, setEmployees] = useState<Employee[]>([]);
    const [departments, setDepartments] = useState<Department[]>([]);
    const [positions, setPositions] = useState<Position[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [query, setQuery] = useState("");

    const [isFormOpen, setIsFormOpen] = useState(false);
    const [editing, setEditing] = useState<Employee | null>(null);
    const [form, setForm] = useState<FormState>(emptyForm);
    const [formError, setFormError] = useState<string | null>(null);
    const [isSaving, setIsSaving] = useState(false);

    const [deleteTarget, setDeleteTarget] = useState<Employee | null>(null);
    const [isDeleting, setIsDeleting] = useState(false);

    const loadEmployees = async () => {
        setIsLoading(true);
        try {
            setEmployees(await employeesApi.list());
            setError(null);
        } catch {
            setError("Failed to load employees.");
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        loadEmployees();
        Promise.all([departmentsApi.list(), positionsApi.list()])
            .then(([deps, pos]) => {
                setDepartments(deps);
                setPositions(pos);
            })
            .catch(() => setError("Failed to load reference data."));
    }, []);

    const filtered = useMemo(() => {
        const q = query.trim().toLowerCase();
        if (!q) return employees;
        return employees.filter((e) =>
            [e.firstName, e.lastName, e.email, e.departmentName, e.positionTitle]
                .join(" ")
                .toLowerCase()
                .includes(q),
        );
    }, [employees, query]);

    const setField = (key: keyof FormState, value: string) => setForm((prev) => ({ ...prev, [key]: value }));

    const openCreate = () => {
        setEditing(null);
        setForm({
            ...emptyForm,
            departmentId: departments[0] ? String(departments[0].id) : "",
            positionId: positions[0] ? String(positions[0].id) : "",
        });
        setFormError(null);
        setIsFormOpen(true);
    };

    const openEdit = (employee: Employee) => {
        setEditing(employee);
        setForm({
            firstName: employee.firstName,
            lastName: employee.lastName,
            email: employee.email,
            hireDate: employee.hireDate,
            departmentId: String(employee.departmentId),
            positionId: String(employee.positionId),
        });
        setFormError(null);
        setIsFormOpen(true);
    };

    const handleSubmit = async (event: FormEvent) => {
        event.preventDefault();
        setFormError(null);

        if (!form.departmentId || !form.positionId) {
            setFormError("Please select a department and a position.");
            return;
        }

        setIsSaving(true);
        const dto = {
            firstName: form.firstName,
            lastName: form.lastName,
            email: form.email,
            hireDate: form.hireDate,
            departmentId: Number(form.departmentId),
            positionId: Number(form.positionId),
        };
        try {
            if (editing) {
                await employeesApi.update(editing.id, dto);
                toast.success("Employee updated.");
            } else {
                await employeesApi.create(dto);
                toast.success("Employee created.");
            }
            setIsFormOpen(false);
            await loadEmployees();
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
            await employeesApi.remove(deleteTarget.id);
            toast.success("Employee deleted.");
            setDeleteTarget(null);
            await loadEmployees();
        } catch (err) {
            toast.error(err instanceof ApiError ? err.message : "Failed to delete employee.");
            setDeleteTarget(null);
        } finally {
            setIsDeleting(false);
        }
    };

    const departmentOptions = departments.map((d) => ({ label: d.name, value: String(d.id) }));
    const positionOptions = positions.map((p) => ({ label: p.title, value: String(p.id) }));

    const columns: Column<Employee>[] = [
        {
            header: "Name",
            render: (e) => `${e.firstName} ${e.lastName}`,
            cellClassName: "font-medium text-primary",
        },
        { header: "Email", render: (e) => e.email },
        { header: "Department", render: (e) => e.departmentName },
        { header: "Position", render: (e) => e.positionTitle },
        {
            header: "Actions",
            align: "right",
            render: (e) => (
                <div className="flex justify-end gap-1">
                    <Button size="sm" color="tertiary" iconLeading={Edit01} onClick={() => openEdit(e)}>
                        Edit
                    </Button>
                    <Button size="sm" color="tertiary-destructive" iconLeading={Trash01} onClick={() => setDeleteTarget(e)}>
                        Delete
                    </Button>
                </div>
            ),
        },
    ];

    return (
        <div className="flex flex-col gap-6">
            <PageHeader
                title="Employees"
                subtitle="Manage the people in your organization."
                action={
                    <Button iconLeading={Plus} onClick={openCreate}>
                        Add employee
                    </Button>
                }
            />

            {error && <ErrorBanner message={error} />}

            <DataTable
                title="All employees"
                columns={columns}
                rows={filtered}
                getKey={(e) => e.id}
                isLoading={isLoading}
                emptyMessage={query ? "No employees match your search." : "No employees yet."}
                search={{ value: query, onChange: setQuery, placeholder: "Search employees" }}
            />

            <ModalShell isOpen={isFormOpen} onOpenChange={setIsFormOpen} title={editing ? "Edit employee" : "Add employee"}>
                <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                    {formError && <ErrorBanner message={formError} />}
                    <div className="grid grid-cols-2 gap-4">
                        <Input label="First name" value={form.firstName} onChange={(v) => setField("firstName", v)} isRequired />
                        <Input label="Last name" value={form.lastName} onChange={(v) => setField("lastName", v)} isRequired />
                    </div>
                    <Input label="Email" type="email" value={form.email} onChange={(v) => setField("email", v)} isRequired />
                    <Input label="Hire date" type="date" value={form.hireDate} onChange={(v) => setField("hireDate", v)} isRequired />
                    <NativeSelect
                        label="Department"
                        options={departmentOptions}
                        value={form.departmentId}
                        onChange={(e) => setField("departmentId", e.target.value)}
                    />
                    <NativeSelect
                        label="Position"
                        options={positionOptions}
                        value={form.positionId}
                        onChange={(e) => setField("positionId", e.target.value)}
                    />
                    <div className="mt-2 flex justify-end gap-3">
                        <Button color="secondary" onClick={() => setIsFormOpen(false)} type="button">
                            Cancel
                        </Button>
                        <Button type="submit" isLoading={isSaving}>
                            {editing ? "Save changes" : "Create employee"}
                        </Button>
                    </div>
                </form>
            </ModalShell>

            <ConfirmModal
                isOpen={deleteTarget !== null}
                title="Delete employee"
                description={`Are you sure you want to delete ${deleteTarget?.firstName} ${deleteTarget?.lastName}? This cannot be undone.`}
                isConfirming={isDeleting}
                onConfirm={confirmDelete}
                onClose={() => setDeleteTarget(null)}
            />
        </div>
    );
};
