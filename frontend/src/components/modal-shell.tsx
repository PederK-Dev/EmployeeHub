import type { PropsWithChildren } from "react";
import { Dialog, Modal, ModalOverlay } from "@/components/application/modals/modal";

interface ModalShellProps extends PropsWithChildren {
    isOpen: boolean;
    onOpenChange: (isOpen: boolean) => void;
    title: string;
    description?: string;
}

export const ModalShell = ({ isOpen, onOpenChange, title, description, children }: ModalShellProps) => {
    return (
        <ModalOverlay isOpen={isOpen} onOpenChange={onOpenChange} isDismissable>
            <Modal>
                <Dialog>
                    <div className="w-full max-w-lg rounded-2xl bg-primary p-6 shadow-xl ring-1 ring-secondary">
                        <div className="mb-5">
                            <h2 className="text-lg font-semibold text-primary">{title}</h2>
                            {description && <p className="mt-1 text-sm text-tertiary">{description}</p>}
                        </div>
                        {children}
                    </div>
                </Dialog>
            </Modal>
        </ModalOverlay>
    );
};
