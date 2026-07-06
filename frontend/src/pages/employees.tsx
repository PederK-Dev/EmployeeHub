import { useEffect, useState, type FormEvent } from "react";
import { Edit01, Plus, Trash01 } from "@untitledui/icons";
import { Button } from "@/components/base/buttons/button";
import { Input } from "@/components/base/input/input";
import { NativeSelect } from "@/components/base/select/select-native";
import { TableCard } from "@/components/application/table/table";
import { ModalShell } from "@/components/modal-shell";
import { ApiError, departmentsApi, employeesApi, positionsApi } from "@/lib/api";
import type { Department, Employee, Position } from "@/lib/types";

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
    const [employees, setEmployees] = useState<Employee[]>([]);
    const [departments, setDepartments] = useState<Department[]>([]);
    const [positions, setPositions] = useState<Position[]>([]);
    const [error, setError] = useState<string | null>(null);

    const [isFormOpen, setIsFormOpen] = useState(false);
    const [editing, setEditing] = useState<Employee | null>(null);
    const [form, setForm] = useState<FormState>(emptyForm);
    const [formError, setFormError] = useState<string | null>(null);
    const [isSaving, setIsSaving] = useState(false);

    const [deleteTarget, setDeleteTarget] = useState<Employee | null>(null);
    const [isDeleting, setIsDeleting] = useState(false);

    const loadEmployees = async () => {
        try {
            setEmployees(await employeesApi.list());
        } catch {
            setError("Failed to load employees.");
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
            } else {
                await employeesApi.create(dto);
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
        setError(null);
        try {
            await employeesApi.remove(deleteTarget.id);
            setDeleteTarget(null);
            await loadEmployees();
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to delete employee.");
            setDeleteTarget(null);
        } finally {
            setIsDeleting(false);
        }
    };

    const departmentOptions = departments.map((d) => ({ label: d.name, value: String(d.id) }));
    const positionOptions = positions.map((p) => ({ label: p.title, value: String(p.id) }));

    return (
        <div className="flex flex-col gap-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-semibold text-primary">Employees</h1>
                    <p className="mt-1 text-sm text-tertiary">Manage the people in your organization.</p>
                </div>
                <Button iconLeading={Plus} onClick={openCreate}>
                    Add employee
                </Button>
            </div>

            {error && (
                <div className="rounded-lg bg-error-primary px-3.5 py-2.5 text-sm text-error-primary ring-1 ring-error_subtle ring-inset">
                    {error}
                </div>
            )}

            <TableCard.Root>
                <TableCard.Header title="All employees" badge={`${employees.length}`} />
                <div className="overflow-x-auto">
                    <table className="w-full text-left text-sm">
                        <thead className="border-b border-secondary bg-secondary">
                            <tr className="text-xs font-semibold text-quaternary">
                                <th className="px-6 py-3">Name</th>
                                <th className="px-6 py-3">Email</th>
                                <th className="px-6 py-3">Department</th>
                                <th className="px-6 py-3">Position</th>
                                <th className="px-6 py-3 text-right">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {employees.length === 0 ? (
                                <tr>
                                    <td className="px-6 py-8 text-center text-tertiary" colSpan={5}>
                                        No employees yet.
                                    </td>
                                </tr>
                            ) : (
                                employees.map((e) => (
                                    <tr key={e.id} className="border-b border-secondary last:border-0">
                                        <td className="px-6 py-4 font-medium text-primary">
                                            {e.firstName} {e.lastName}
                                        </td>
                                        <td className="px-6 py-4 text-tertiary">{e.email}</td>
                                        <td className="px-6 py-4 text-tertiary">{e.departmentName}</td>
                                        <td className="px-6 py-4 text-tertiary">{e.positionTitle}</td>
                                        <td className="px-6 py-4">
                                            <div className="flex justify-end gap-1">
                                                <Button size="sm" color="tertiary" iconLeading={Edit01} onClick={() => openEdit(e)}>
                                                    Edit
                                                </Button>
                                                <Button
                                                    size="sm"
                                                    color="tertiary-destructive"
                                                    iconLeading={Trash01}
                                                    onClick={() => setDeleteTarget(e)}
                                                >
                                                    Delete
                                                </Button>
                                            </div>
                                        </td>
                                    </tr>
                                ))
                            )}
                        </tbody>
                    </table>
                </div>
            </TableCard.Root>

            <ModalShell
                isOpen={isFormOpen}
                onOpenChange={setIsFormOpen}
                title={editing ? "Edit employee" : "Add employee"}
            >
                <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                    {formError && (
                        <div className="rounded-lg bg-error-primary px-3.5 py-2.5 text-sm text-error-primary ring-1 ring-error_subtle ring-inset">
                            {formError}
                        </div>
                    )}
                    <div className="grid grid-cols-2 gap-4">
                        <Input label="First name" value={form.firstName} onChange={(v) => setField("firstName", v)} isRequired />
                        <Input label="Last name" value={form.lastName} onChange={(v) => setField("lastName", v)} isRequired />
                    </div>
                    <Input label="Email" type="email" value={form.email} onChange={(v) => setField("email", v)} isRequired />
                    <Input
                        label="Hire date"
                        type="date"
                        value={form.hireDate}
                        onChange={(v) => setField("hireDate", v)}
                        isRequired
                    />
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

            <ModalShell
                isOpen={deleteTarget !== null}
                onOpenChange={(open) => !open && setDeleteTarget(null)}
                title="Delete employee"
                description={`Are you sure you want to delete ${deleteTarget?.firstName} ${deleteTarget?.lastName}? This cannot be undone.`}
            >
                <div className="flex justify-end gap-3">
                    <Button color="secondary" onClick={() => setDeleteTarget(null)} type="button">
                        Cancel
                    </Button>
                    <Button color="primary-destructive" isLoading={isDeleting} onClick={confirmDelete}>
                        Delete
                    </Button>
                </div>
            </ModalShell>
        </div>
    );
};
