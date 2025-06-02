import React, {
  ReactNode,
  useRef,
  useState,
  Dispatch,
  SetStateAction,
} from "react";
import { Modal, ModalHeader, ModalBody, ModalFooter, Button } from "reactstrap";
import { useModal } from "react-modal-hook";
import { isBoolean, isFunction } from "../util/TypeChecks";

type ButtonText = "Confirm" | "Cancel" | "OK";

interface Props<T> {
  title: ReactNode;
  message:
    | ReactNode
    | ((setReturnValue: Dispatch<SetStateAction<T | undefined>>) => ReactNode);
  canConfirm?: boolean | ((returnValue: T) => boolean);
  buttons?: ButtonText[];
}

export const useConfirmationDialog = <T extends any = undefined>(
  props: Props<T>,
  dependencies: any[] = [],
) => {
  const promiseRef = useRef<Promise<[boolean, T]>>();
  const resolveRef = useRef<(value: [boolean, T]) => void>();
  const [returnValue, setReturnValue] = useState<T>();

  const resetState = () => {
    promiseRef.current = undefined;
    resolveRef.current = undefined;
    setReturnValue(undefined);
  };

  const confirm = () => {
    resolveRef.current && resolveRef.current([true, returnValue as T]);
    resetState();
  };

  const dismiss = () => {
    resolveRef.current && resolveRef.current([false, undefined as T]);
    resetState();
  };

  const buttons = props.buttons ?? ["Confirm", "Cancel"];

  const [showModal, hideModal] = useModal(
    () => (
      <Modal isOpen={true}>
        <ModalHeader>{props.title}</ModalHeader>
        <ModalBody>
          {isFunction(props.message)
            ? props.message(setReturnValue)
            : props.message}
        </ModalBody>
        <ModalFooter>
          {buttons.includes("Confirm") && (
            <Button
              color="primary"
              onClick={() => {
                confirm();
                hideModal();
              }}
              disabled={
                props.canConfirm === undefined
                  ? false
                  : isBoolean(props.canConfirm)
                    ? !props.canConfirm
                    : !props.canConfirm(returnValue)
              }
            >
              Confirm
            </Button>
          )}{" "}
          {buttons.some((b) => b === "Cancel" || b === "OK") && (
            <Button
              color="primary"
              onClick={() => {
                dismiss();
                hideModal();
              }}
            >
              {buttons.includes("Cancel") ? " Cancel" : "OK"}
            </Button>
          )}
        </ModalFooter>
      </Modal>
    ),
    [...dependencies, returnValue, setReturnValue],
  );

  const getConfirmation = () => {
    let promise =
      promiseRef.current ||
      new Promise<[boolean, T]>((resolve) => {
        resolveRef.current = resolve;
        showModal();
      });
    if (promiseRef.current === undefined) {
      promiseRef.current = promise;
    }
    return promise;
  };

  return [getConfirmation];
};
