import { useEffect, useState, type FormEvent } from "react";
import { Edit01, Plus, Trash01 } from "@untitledui/icons";
import { Button } from "@/components/base/buttons/button";
import { Input } from "@/components/base/input/input";
import { TextArea } from "@/components/base/textarea/textarea";
import { TableCard } from "@/components/application/table/table";
import { ModalShell } from "@/components/modal-shell";
import { ApiError, departmentsApi } from "@/lib/api";
import type { Department } from "@/lib/types";

export const Departments = () => {
    const [departments, setDepartments] = useState<Department[]>([]);
    const [error, setError] = useState<string | null>(null);

    const [isFormOpen, setIsFormOpen] = useState(false);
    const [editing, setEditing] = useState<Department | null>(null);
    const [name, setName] = useState("");
    const [description, setDescription] = useState("");
    const [formError, setFormError] = useState<string | null>(null);
    const [isSaving, setIsSaving] = useState(false);

    const [deleteTarget, setDeleteTarget] = useState<Department | null>(null);
    const [isDeleting, setIsDeleting] = useState(false);

    const load = async () => {
        try {
            setDepartments(await departmentsApi.list());
        } catch {
            setError("Failed to load departments.");
        }
    };

    useEffect(() => {
        load();
    }, []);

    const openCreate = () => {
        setEditing(null);
        setName("");
        setDescription("");
        setFormError(null);
        setIsFormOpen(true);
    };

    const openEdit = (department: Department) => {
        setEditing(department);
        setName(department.name);
        setDescription(department.description ?? "");
        setFormError(null);
        setIsFormOpen(true);
    };

    const handleSubmit = async (event: FormEvent) => {
        event.preventDefault();
        setFormError(null);
        setIsSaving(true);
        const dto = { name, description: description.trim() === "" ? null : description };
        try {
            if (editing) {
                await departmentsApi.update(editing.id, dto);
            } else {
                await departmentsApi.create(dto);
            }
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
        setError(null);
        try {
            await departmentsApi.remove(deleteTarget.id);
            setDeleteTarget(null);
            await load();
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Failed to delete department.");
            setDeleteTarget(null);
        } finally {
            setIsDeleting(false);
        }
    };

    return (
        <div className="flex flex-col gap-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-semibold text-primary">Departments</h1>
                    <p className="mt-1 text-sm text-tertiary">Manage your organization's departments.</p>
                </div>
                <Button iconLeading={Plus} onClick={openCreate}>
                    Add department
                </Button>
            </div>

            {error && (
                <div className="rounded-lg bg-error-primary px-3.5 py-2.5 text-sm text-error-primary ring-1 ring-error_subtle ring-inset">
                    {error}
                </div>
            )}

            <TableCard.Root>
                <TableCard.Header title="All departments" badge={`${departments.length}`} />
                <div className="overflow-x-auto">
                    <table className="w-full text-left text-sm">
                        <thead className="border-b border-secondary bg-secondary">
                            <tr className="text-xs font-semibold text-quaternary">
                                <th className="px-6 py-3">Name</th>
                                <th className="px-6 py-3">Description</th>
                                <th className="px-6 py-3 text-right">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {departments.length === 0 ? (
                                <tr>
                                    <td className="px-6 py-8 text-center text-tertiary" colSpan={3}>
                                        No departments yet.
                                    </td>
                                </tr>
                            ) : (
                                departments.map((d) => (
                                    <tr key={d.id} className="border-b border-secondary last:border-0">
                                        <td className="px-6 py-4 font-medium text-primary">{d.name}</td>
                                        <td className="px-6 py-4 text-tertiary">{d.description ?? "—"}</td>
                                        <td className="px-6 py-4">
                                            <div className="flex justify-end gap-1">
                                                <Button size="sm" color="tertiary" iconLeading={Edit01} onClick={() => openEdit(d)}>
                                                    Edit
                                                </Button>
                                                <Button
                                                    size="sm"
                                                    color="tertiary-destructive"
                                                    iconLeading={Trash01}
                                                    onClick={() => setDeleteTarget(d)}
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
                title={editing ? "Edit department" : "Add department"}
            >
                <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                    {formError && (
                        <div className="rounded-lg bg-error-primary px-3.5 py-2.5 text-sm text-error-primary ring-1 ring-error_subtle ring-inset">
                            {formError}
                        </div>
                    )}
                    <Input label="Name" value={name} onChange={setName} placeholder="Engineering" isRequired />
                    <TextArea
                        label="Description"
                        value={description}
                        onChange={setDescription}
                        placeholder="What does this department do?"
                    />
                    <div className="mt-2 flex justify-end gap-3">
                        <Button color="secondary" onClick={() => setIsFormOpen(false)} type="button">
                            Cancel
                        </Button>
                        <Button type="submit" isLoading={isSaving}>
                            {editing ? "Save changes" : "Create department"}
                        </Button>
                    </div>
                </form>
            </ModalShell>

            <ModalShell
                isOpen={deleteTarget !== null}
                onOpenChange={(open) => !open && setDeleteTarget(null)}
                title="Delete department"
                description={`Are you sure you want to delete "${deleteTarget?.name}"? This cannot be undone.`}
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
