import type {
    AuthResponse,
    CreateDepartment,
    CreateEmployee,
    Department,
    Employee,
    Position,
    User,
} from "./types";

const BASE_URL = (import.meta.env.VITE_API_URL as string | undefined) ?? "http://localhost:5023/api";

export const TOKEN_KEY = "eh_token";
export const USER_KEY = "eh_user";

export class ApiError extends Error {
    status: number;

    constructor(status: number, message: string) {
        super(message);
        this.status = status;
        this.name = "ApiError";
    }
}

// Registered by the auth provider so a 401 anywhere triggers a logout.
let onUnauthorized: (() => void) | null = null;
export const setUnauthorizedHandler = (fn: (() => void) | null) => {
    onUnauthorized = fn;
};

interface RequestOptions {
    method?: string;
    body?: unknown;
    /** Skip the automatic logout-on-401 (used by the login call itself). */
    skipAuthRedirect?: boolean;
}

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
    const token = localStorage.getItem(TOKEN_KEY);

    const headers: Record<string, string> = {};
    if (options.body !== undefined) headers["Content-Type"] = "application/json";
    if (token) headers["Authorization"] = `Bearer ${token}`;

    const response = await fetch(`${BASE_URL}${path}`, {
        method: options.method ?? "GET",
        headers,
        body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
    });

    if (response.status === 401 && !options.skipAuthRedirect) {
        onUnauthorized?.();
        throw new ApiError(401, "Your session has expired. Please sign in again.");
    }

    if (!response.ok) {
        let message = response.statusText;
        try {
            const problem = await response.json();
            message = problem?.error ?? problem?.title ?? message;
        } catch {
            // response had no JSON body; keep the status text
        }
        throw new ApiError(response.status, message);
    }

    if (response.status === 204) return undefined as T;

    return (await response.json()) as T;
}

export const authApi = {
    login: (email: string, password: string) =>
        request<AuthResponse>("/auth/login", {
            method: "POST",
            body: { email, password },
            skipAuthRedirect: true,
        }),
    me: () => request<User>("/auth/me"),
};

export const departmentsApi = {
    list: () => request<Department[]>("/departments"),
    create: (dto: CreateDepartment) => request<Department>("/departments", { method: "POST", body: dto }),
    update: (id: number, dto: CreateDepartment) =>
        request<Department>(`/departments/${id}`, { method: "PUT", body: dto }),
    remove: (id: number) => request<void>(`/departments/${id}`, { method: "DELETE" }),
};

export const positionsApi = {
    list: () => request<Position[]>("/positions"),
};

export const employeesApi = {
    list: () => request<Employee[]>("/employees"),
    create: (dto: CreateEmployee) => request<Employee>("/employees", { method: "POST", body: dto }),
    update: (id: number, dto: CreateEmployee) =>
        request<Employee>(`/employees/${id}`, { method: "PUT", body: dto }),
    remove: (id: number) => request<void>(`/employees/${id}`, { method: "DELETE" }),
};
