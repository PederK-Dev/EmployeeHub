import { useEffect, useMemo, useState, type FormEvent } from "react";
import { Check, Plus, Trash01, XClose } from "@untitledui/icons";
import { Button } from "@/components/base/buttons/button";
import { Input } from "@/components/base/input/input";
import { NativeSelect } from "@/components/base/select/select-native";
import { TextArea } from "@/components/base/textarea/textarea";
import { ErrorBanner } from "@/components/banner";
import { ConfirmModal } from "@/components/confirm-modal";
import { DataTable, type Column } from "@/components/data-table";
import { ModalShell } from "@/components/modal-shell";
import { PageHeader } from "@/components/page-header";
import { LeaveStatusBadge } from "@/components/status-badge";
import { ApiError, employeesApi, leaveApi } from "@/lib/api";
import { LEAVE_TYPES, type Employee, type LeaveRequest, type LeaveStatus, type LeaveType } from "@/lib/types";
import { useAuth } from "@/providers/auth-provider";
import { useToast } from "@/providers/toast-provider";

interface FormState {
    employeeId: string;
    type: LeaveType;
    startDate: string;
    endDate: string;
    reason: string;
}

const emptyForm: FormState = { employeeId: "", type: "Annual", startDate: "", endDate: "", reason: "" };

export const LeaveRequests = () => {
    const toast = useToast();
    const { user } = useAuth();
    const canManage = user?.role === "Admin" || user?.role === "Manager";
    const [requests, setRequests] = useState<LeaveRequest[]>([]);
    const [employees, setEmployees] = useState<Employee[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [query, setQuery] = useState("");
    const [actingId, setActingId] = useState<number | null>(null);

    const [isFormOpen, setIsFormOpen] = useState(false);
    const [form, setForm] = useState<FormState>(emptyForm);
    const [formError, setFormError] = useState<string | null>(null);
    const [isSaving, setIsSaving] = useState(false);

    const [deleteTarget, setDeleteTarget] = useState<LeaveRequest | null>(null);
    const [isDeleting, setIsDeleting] = useState(false);

    const load = async () => {
        setIsLoading(true);
        try {
            setRequests(await leaveApi.list());
            setError(null);
        } catch {
            setError("Failed to load leave requests.");
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        load();

        // The directory is manager-and-up only. Everyone else can just file for themselves,
        // so there is no picker to populate.
        if (!canManage) return;

        employeesApi
            .list()
            .then(setEmployees)
            .catch(() => setError("Failed to load employees."));
    }, [canManage]);

    const filtered = useMemo(() => {
        const q = query.trim().toLowerCase();
        if (!q) return requests;
        return requests.filter((r) =>
            [r.employeeName, r.type, r.status, r.reason ?? ""].join(" ").toLowerCase().includes(q),
        );
    }, [requests, query]);

    const setField = (key: keyof FormState, value: string) => setForm((prev) => ({ ...prev, [key]: value }));

    const openCreate = () => {
        const defaultEmployeeId = canManage ? (employees[0] ? String(employees[0].id) : "") : String(user?.employeeId ?? "");
        setForm({ ...emptyForm, employeeId: defaultEmployeeId });
        setFormError(null);
        setIsFormOpen(true);
    };

    const handleSubmit = async (event: FormEvent) => {
        event.preventDefault();
        setFormError(null);

        if (!form.employeeId) {
            setFormError("Please select an employee.");
            return;
        }
        if (form.endDate < form.startDate) {
            setFormError("End date cannot be earlier than start date.");
            return;
        }

        setIsSaving(true);
        try {
            await leaveApi.create({
                employeeId: Number(form.employeeId),
                type: form.type,
                startDate: form.startDate,
                endDate: form.endDate,
                reason: form.reason.trim() === "" ? null : form.reason,
            });
            toast.success("Leave request submitted.");
            setIsFormOpen(false);
            await load();
        } catch (err) {
            setFormError(err instanceof ApiError ? err.message : "Something went wrong.");
        } finally {
            setIsSaving(false);
        }
    };

    const updateStatus = async (request: LeaveRequest, status: LeaveStatus) => {
        setActingId(request.id);
        try {
            await leaveApi.updateStatus(request.id, status);
            toast.success(`Request ${status.toLowerCase()}.`);
            await load();
        } catch (err) {
            toast.error(err instanceof ApiError ? err.message : "Failed to update request.");
        } finally {
            setActingId(null);
        }
    };

    const confirmDelete = async () => {
        if (!deleteTarget) return;
        setIsDeleting(true);
        try {
            await leaveApi.remove(deleteTarget.id);
            toast.success("Leave request deleted.");
            setDeleteTarget(null);
            await load();
        } catch (err) {
            toast.error(err instanceof ApiError ? err.message : "Failed to delete request.");
            setDeleteTarget(null);
        } finally {
            setIsDeleting(false);
        }
    };

    const employeeOptions = employees.map((e) => ({ label: `${e.firstName} ${e.lastName}`, value: String(e.id) }));
    const typeOptions = LEAVE_TYPES.map((t) => ({ label: t, value: t }));

    const columns: Column<LeaveRequest>[] = [
        { header: "Employee", render: (r) => r.employeeName, cellClassName: "font-medium text-primary" },
        { header: "Type", render: (r) => r.type },
        { header: "Dates", render: (r) => `${r.startDate} → ${r.endDate}` },
        { header: "Status", render: (r) => <LeaveStatusBadge status={r.status} /> },
        {
            header: "Actions",
            align: "right",
            render: (r) =>
                canManage ? (
                    <div className="flex justify-end gap-1">
                        {r.status === "Pending" && (
                            <>
                                <Button
                                    size="sm"
                                    color="tertiary"
                                    iconLeading={Check}
                                    isLoading={actingId === r.id}
                                    onClick={() => updateStatus(r, "Approved")}
                                >
                                    Approve
                                </Button>
                                <Button
                                    size="sm"
                                    color="tertiary-destructive"
                                    iconLeading={XClose}
                                    isDisabled={actingId === r.id}
                                    onClick={() => updateStatus(r, "Rejected")}
                                >
                                    Reject
                                </Button>
                            </>
                        )}
                        <Button size="sm" color="tertiary-destructive" iconLeading={Trash01} onClick={() => setDeleteTarget(r)}>
                            Delete
                        </Button>
                    </div>
                ) : (
                    <span className="text-quaternary">—</span>
                ),
        },
    ];

    return (
        <div className="flex flex-col gap-6">
            <PageHeader
                title="Leave requests"
                subtitle="Review and manage time-off requests."
                action={
                    <Button iconLeading={Plus} onClick={openCreate} isDisabled={canManage ? employees.length === 0 : !user?.employeeId}>
                        New request
                    </Button>
                }
            />

            {error && <ErrorBanner message={error} />}

            <DataTable
                title={canManage ? "All requests" : "My requests"}
                columns={columns}
                rows={filtered}
                getKey={(r) => r.id}
                isLoading={isLoading}
                emptyMessage={query ? "No requests match your search." : "No leave requests yet."}
                search={{ value: query, onChange: setQuery, placeholder: "Search requests" }}
            />

            <ModalShell isOpen={isFormOpen} onOpenChange={setIsFormOpen} title="New leave request">
                <form onSubmit={handleSubmit} className="flex flex-col gap-4">
                    {formError && <ErrorBanner message={formError} />}
                    {canManage && (
                        <NativeSelect
                            label="Employee"
                            options={employeeOptions}
                            value={form.employeeId}
                            onChange={(e) => setField("employeeId", e.target.value)}
                        />
                    )}
                    <NativeSelect
                        label="Type"
                        options={typeOptions}
                        value={form.type}
                        onChange={(e) => setField("type", e.target.value)}
                    />
                    <div className="grid grid-cols-2 gap-4">
                        <Input label="Start date" type="date" value={form.startDate} onChange={(v) => setField("startDate", v)} isRequired />
                        <Input label="End date" type="date" value={form.endDate} onChange={(v) => setField("endDate", v)} isRequired />
                    </div>
                    <TextArea
                        label="Reason"
                        value={form.reason}
                        onChange={(v) => setField("reason", v)}
                        placeholder="Optional note"
                    />
                    <div className="mt-2 flex justify-end gap-3">
                        <Button color="secondary" onClick={() => setIsFormOpen(false)} type="button">
                            Cancel
                        </Button>
                        <Button type="submit" isLoading={isSaving}>
                            Submit request
                        </Button>
                    </div>
                </form>
            </ModalShell>

            <ConfirmModal
                isOpen={deleteTarget !== null}
                title="Delete leave request"
                description={`Delete the ${deleteTarget?.type.toLowerCase()} leave request for ${deleteTarget?.employeeName}? This cannot be undone.`}
                isConfirming={isDeleting}
                onConfirm={confirmDelete}
                onClose={() => setDeleteTarget(null)}
            />
        </div>
    );
};
