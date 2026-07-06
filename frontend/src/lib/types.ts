export type UserRole = "Employee" | "Manager" | "Admin";

export interface User {
    id: number;
    email: string;
    role: UserRole;
    employeeId: number | null;
}

export interface AuthResponse {
    token: string;
    expiresAt: string;
    user: User;
}

export interface Department {
    id: number;
    name: string;
    description: string | null;
}

export interface Position {
    id: number;
    title: string;
    description: string | null;
}

export interface Employee {
    id: number;
    firstName: string;
    lastName: string;
    email: string;
    hireDate: string;
    departmentId: number;
    departmentName: string;
    positionId: number;
    positionTitle: string;
}

export interface CreateDepartment {
    name: string;
    description?: string | null;
}

export interface CreateEmployee {
    firstName: string;
    lastName: string;
    email: string;
    hireDate: string;
    departmentId: number;
    positionId: number;
}

export interface CreatePosition {
    title: string;
    description?: string | null;
}

export type LeaveType = "Annual" | "Sick" | "Unpaid" | "Parental" | "Bereavement";
export type LeaveStatus = "Pending" | "Approved" | "Rejected" | "Cancelled";

export const LEAVE_TYPES: LeaveType[] = ["Annual", "Sick", "Unpaid", "Parental", "Bereavement"];
export const LEAVE_STATUSES: LeaveStatus[] = ["Pending", "Approved", "Rejected", "Cancelled"];
export const USER_ROLES: UserRole[] = ["Employee", "Manager", "Admin"];

export interface LeaveRequest {
    id: number;
    employeeId: number;
    employeeName: string;
    type: LeaveType;
    status: LeaveStatus;
    startDate: string;
    endDate: string;
    reason: string | null;
    requestedAt: string;
}

export interface CreateLeaveRequest {
    employeeId: number;
    type: LeaveType;
    startDate: string;
    endDate: string;
    reason?: string | null;
}

export interface CreateUser {
    email: string;
    password: string;
    role: UserRole;
    employeeId?: number | null;
}

export interface DashboardStats {
    departmentCount: number;
    employeeCount: number;
    positionCount: number;
    pendingLeaveCount: number;
    leaveByStatus: Record<string, number>;
    recentEmployees: Employee[];
}
