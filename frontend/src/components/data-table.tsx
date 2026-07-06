import type { ReactNode } from "react";
import { SearchLg } from "@untitledui/icons";
import { Input } from "@/components/base/input/input";
import { TableCard } from "@/components/application/table/table";
import { cx } from "@/utils/cx";

export interface Column<T> {
    header: string;
    render: (row: T) => ReactNode;
    align?: "left" | "right";
    /** Extra classes for the cell (e.g. text emphasis). */
    cellClassName?: string;
}

interface DataTableProps<T> {
    title: string;
    columns: Column<T>[];
    rows: T[];
    getKey: (row: T) => string | number;
    isLoading?: boolean;
    emptyMessage?: string;
    /** Optional controlled search box shown in the header. */
    search?: { value: string; onChange: (value: string) => void; placeholder?: string };
    /** Optional trailing content in the header, e.g. an "Add" button. */
    toolbar?: ReactNode;
}

export function DataTable<T>({
    title,
    columns,
    rows,
    getKey,
    isLoading = false,
    emptyMessage = "Nothing here yet.",
    search,
    toolbar,
}: DataTableProps<T>) {
    return (
        <TableCard.Root>
            <TableCard.Header
                title={title}
                badge={`${rows.length}`}
                contentTrailing={
                    <div className="flex w-full items-center gap-3 md:w-auto">
                        {search && (
                            <div className="w-full md:w-64">
                                <Input
                                    size="sm"
                                    icon={SearchLg}
                                    aria-label="Search"
                                    placeholder={search.placeholder ?? "Search"}
                                    value={search.value}
                                    onChange={search.onChange}
                                />
                            </div>
                        )}
                        {toolbar}
                    </div>
                }
            />
            <div className="overflow-x-auto">
                <table className="w-full text-left text-sm">
                    <thead className="border-b border-secondary bg-secondary">
                        <tr className="text-xs font-semibold text-quaternary">
                            {columns.map((col, i) => (
                                <th key={i} className={cx("px-6 py-3", col.align === "right" && "text-right")}>
                                    {col.header}
                                </th>
                            ))}
                        </tr>
                    </thead>
                    <tbody>
                        {isLoading ? (
                            <tr>
                                <td className="px-6 py-10 text-center" colSpan={columns.length}>
                                    <div className="flex items-center justify-center gap-2 text-tertiary">
                                        <svg className="size-5 animate-spin" viewBox="0 0 20 20" fill="none">
                                            <circle className="stroke-current opacity-30" cx="10" cy="10" r="8" strokeWidth="2" />
                                            <circle
                                                className="origin-center stroke-current"
                                                cx="10"
                                                cy="10"
                                                r="8"
                                                strokeWidth="2"
                                                strokeDasharray="12.5 50"
                                                strokeLinecap="round"
                                            />
                                        </svg>
                                        <span>Loading…</span>
                                    </div>
                                </td>
                            </tr>
                        ) : rows.length === 0 ? (
                            <tr>
                                <td className="px-6 py-10 text-center text-tertiary" colSpan={columns.length}>
                                    {emptyMessage}
                                </td>
                            </tr>
                        ) : (
                            rows.map((row) => (
                                <tr key={getKey(row)} className="border-b border-secondary last:border-0">
                                    {columns.map((col, i) => (
                                        <td
                                            key={i}
                                            className={cx(
                                                "px-6 py-4 text-tertiary",
                                                col.align === "right" && "text-right",
                                                col.cellClassName,
                                            )}
                                        >
                                            {col.render(row)}
                                        </td>
                                    ))}
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>
        </TableCard.Root>
    );
}
