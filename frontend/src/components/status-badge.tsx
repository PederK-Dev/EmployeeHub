import { Badge } from "@/components/base/badges/badges";
import type { LeaveStatus } from "@/lib/types";

const colorByStatus: Record<LeaveStatus, "warning" | "success" | "error" | "gray"> = {
    Pending: "warning",
    Approved: "success",
    Rejected: "error",
    Cancelled: "gray",
};

export const LeaveStatusBadge = ({ status }: { status: LeaveStatus }) => (
    <Badge size="sm" type="pill-color" color={colorByStatus[status]}>
        {status}
    </Badge>
);
