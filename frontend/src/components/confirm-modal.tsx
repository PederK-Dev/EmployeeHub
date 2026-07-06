import { Button } from "@/components/base/buttons/button";
import { ModalShell } from "@/components/modal-shell";

interface ConfirmModalProps {
    isOpen: boolean;
    title: string;
    description?: string;
    confirmLabel?: string;
    isConfirming?: boolean;
    onConfirm: () => void;
    onClose: () => void;
}

export const ConfirmModal = ({
    isOpen,
    title,
    description,
    confirmLabel = "Delete",
    isConfirming = false,
    onConfirm,
    onClose,
}: ConfirmModalProps) => {
    return (
        <ModalShell isOpen={isOpen} onOpenChange={(open) => !open && onClose()} title={title} description={description}>
            <div className="flex justify-end gap-3">
                <Button color="secondary" type="button" onClick={onClose}>
                    Cancel
                </Button>
                <Button color="primary-destructive" isLoading={isConfirming} onClick={onConfirm}>
                    {confirmLabel}
                </Button>
            </div>
        </ModalShell>
    );
};
