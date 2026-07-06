import { useEffect, useMemo, useState, type FormEvent } from "react";
import { Edit01, Plus, Trash01 } from "@untitledui/icons";
import { Button } from "@/components/base/buttons/button";
import { Input } from "@/components/base/input/input";
import { TextArea } from "@/components/base/textarea/textarea";
import { ErrorBanner } from "@/components/banner";
import { ConfirmModal } from "@/components/confirm-modal";
import { DataTable, type Column } from "@/components/data-table";
import { ModalShell } from "@/components/modal-shell";
import { PageHeader } from "@/components/page-header";
import { ApiError, positionsApi } from "@/lib/api";
import type { Position } from "@/lib/types";
import { useToast } from "@/providers/toast-provider";

export const Positions = () => {
    const toast = useToast();
    const [positions, setPositions] = useState<Position[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [query, setQuery] = useState("");

    const [isFormOpen, setIsFormOpen] = useState(false);
    const [editing, setEditing] = useState<Position | null>(null);
    const [title, setTitle] = useState("");
    const [description, setDescription] = useState("");
    const [formError, setFormError] = useState<string | null>(null);
    const [isSaving, setIsSaving] = useState(false);

    const [deleteTarget, setDeleteTarget] = useState<Position | null>(null);
    const [isDeleting, setIsDeleting] = useState(false);

    const load = async () => {
        setIsLoading(true);
        try {
            setPositions(await positionsApi.list());
            setError(null);
        } catch {
            setError("Failed to load positions.");
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        load();
    }, []);

    const filtered = useMemo(() => {
        const q = query.trim().toLowerCase();
        if (!q) return positions;
        return positions.filter(
            (p) => p.title.toLowerCase().includes(q) || (p.description ?? "").toLowerCase().includes(q),
        );
    }, [positions, query]);

    const openCreate = () => {
        setEditing(null);
        setTitle("");
        setDescription("");
        setFormError(null);
        setIsFormOpen(true);
    };

    const openEdit = (position: Position) => {
        setEditing(position);
        setTitle(position.title);
        setDescription(position.description ?? "");
        setFormError(null);
        setIsFormOpen(true);
    };

    const handleSubmit = async (event: FormEvent) => {
        event.preventDefault();
        setFormError(null);
        setIsSaving(true);
        const dto = { title, description: description.trim() === "" ? null : description };
        try {
            if (editing) {
                await positionsApi.update(editing.id, dto);
                toast.success("Position updated.");
            } else {
                await positionsApi.create(dto);
                toast.success("Position created.");
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
        try {
            await positionsApi.remove(deleteTarget.id);
            toast.success("Position deleted.");
            setDeleteTarget(null);
            await load();
        } catch (err) {
            toast.error(err instanceof ApiError ? err.message : "Failed to delete position.");
            setDeleteTarget(null);
        } finally {
            setIsDeleting(false);
        }
    };

    const columns: Column<Position>[] = [
        { header: "Title", render: (p) => p.title, cellClassName: "font-medium text-primary" },
        { header: "Description", render: (p) => p.description ?? "—" },
        {
            header: "Actions",
            align: "right",
            render: (p) => (
                <div className="flex justify-end gap-1">
                    <Button size="sm" color="tertiary" iconLeading={Edit01} onClick={() => openEdit(p)}>
                        Edit
                    </Button>
                    <Button size="sm" color="tertiary-destructive" iconLeading={Trash01} onClick={() => setDeleteTarget(p)}>
                        Delete
                    </Button>
                </div>
            ),
        },
    ];

    return (
        <div className="flex flex-col gap-6">
            <PageHeader
                title="Positions"
                subtitle="Manage the job titles used across your organization."
                action={
                    <Button iconLeading={Plus} onClick={openCreate}>
                        Add position
                    </Button>
                }
            />

            {error && <ErrorBanner message={error} />}

            <DataTable
                title="All positions"
                columns={columns}
                rows={filtered}
                getKey={(p) => p.id}
                isLoading={isLoading}
                emptyMessage={query ? "No positions match your search." : "No positions yet."}
                search={{ value: query, onChange: setQuery, placeholder: "Search positions" }}
            />

            <ModalShell isOpen={isFormOpen} onOpenChange={setIsFormOpen} title={editing ? "Edit position" : "Add position"}>
                <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                    {formError && <ErrorBanner message={formError} />}
                    <Input label="Title" value={title} onChange={setTitle} placeholder="Software Engineer" isRequired />
                    <TextArea
                        label="Description"
                        value={description}
                        onChange={setDescription}
                        placeholder="What does this role involve?"
                    />
                    <div className="mt-2 flex justify-end gap-3">
                        <Button color="secondary" onClick={() => setIsFormOpen(false)} type="button">
                            Cancel
                        </Button>
                        <Button type="submit" isLoading={isSaving}>
                            {editing ? "Save changes" : "Create position"}
                        </Button>
                    </div>
                </form>
            </ModalShell>

            <ConfirmModal
                isOpen={deleteTarget !== null}
                title="Delete position"
                description={`Are you sure you want to delete "${deleteTarget?.title}"? This cannot be undone.`}
                isConfirming={isDeleting}
                onConfirm={confirmDelete}
                onClose={() => setDeleteTarget(null)}
            />
        </div>
    );
};
