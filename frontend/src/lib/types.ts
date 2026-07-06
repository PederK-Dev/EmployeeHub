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
