import { useState, type FormEvent } from "react";
import { Building07 } from "@untitledui/icons";
import { Navigate, useLocation, useNavigate } from "react-router";
import { Button } from "@/components/base/buttons/button";
import { Input } from "@/components/base/input/input";
import { ApiError } from "@/lib/api";
import { useAuth } from "@/providers/auth-provider";

export const Login = () => {
    const { login, isAuthenticated } = useAuth();
    const navigate = useNavigate();
    const location = useLocation();
    const from = (location.state as { from?: { pathname?: string } } | null)?.from?.pathname ?? "/";

    const [email, setEmail] = useState("admin@employeehub.local");
    const [password, setPassword] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(false);

    if (isAuthenticated) {
        return <Navigate to={from} replace />;
    }

    const handleSubmit = async (event: FormEvent) => {
        event.preventDefault();
        setError(null);
        setIsLoading(true);
        try {
            await login(email, password);
            navigate(from, { replace: true });
        } catch (err) {
            setError(err instanceof ApiError ? err.message : "Unable to sign in. Please try again.");
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="flex min-h-dvh items-center justify-center bg-secondary px-4">
            <div className="w-full max-w-sm">
                <div className="mb-8 flex flex-col items-center gap-3 text-center">
                    <div className="flex size-12 items-center justify-center rounded-xl bg-brand-solid text-white">
                        <Building07 className="size-6" />
                    </div>
                    <div>
                        <h1 className="text-xl font-semibold text-primary">Sign in to EmployeeHub</h1>
                        <p className="mt-1 text-sm text-tertiary">Welcome back. Please enter your details.</p>
                    </div>
                </div>

                <form
                    onSubmit={handleSubmit}
                    className="flex flex-col gap-4 rounded-2xl bg-primary p-6 shadow-xs ring-1 ring-secondary"
                >
                    {error && (
                        <div className="rounded-lg bg-error-primary px-3.5 py-2.5 text-sm text-error-primary ring-1 ring-error_subtle ring-inset">
                            {error}
                        </div>
                    )}

                    <Input
                        label="Email"
                        type="email"
                        value={email}
                        onChange={setEmail}
                        placeholder="you@company.com"
                        isRequired
                    />

                    <Input
                        label="Password"
                        type="password"
                        value={password}
                        onChange={setPassword}
                        placeholder="••••••••"
                        isRequired
                    />

                    <Button type="submit" size="lg" isLoading={isLoading} className="mt-2 w-full">
                        Sign in
                    </Button>
                </form>

                <p className="mt-4 text-center text-xs text-tertiary">
                    Seeded admin: admin@employeehub.local / Admin123!
                </p>
            </div>
        </div>
    );
};
