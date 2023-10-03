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

interface Props<T> {
  title: ReactNode;
  message:
    | ReactNode
    | ((setReturnValue: Dispatch<SetStateAction<T | undefined>>) => ReactNode);
  canConfirm?: boolean | ((returnValue: T) => boolean);
}

export const useConfirmationDialog = <T extends any = undefined>(
  props: Props<T>,
  dependencies: any[] = []
) => {
  const promiseRef = useRef<Promise<[boolean, T]>>();
  const resolveRef = useRef<(value: [boolean, T]) => void>();
  const [returnValue, setReturnValue] = useState<T>();

  const confirm = () => {
    resolveRef.current && resolveRef.current([true, returnValue as T]);
    promiseRef.current = undefined;
    resolveRef.current = undefined;
  };

  const dismiss = () => {
    resolveRef.current && resolveRef.current([false, undefined as T]);
    promiseRef.current = undefined;
    resolveRef.current = undefined;
  };

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
          </Button>{" "}
          <Button
            color="primary"
            onClick={() => {
              dismiss();
              hideModal();
            }}
          >
            Cancel
          </Button>
        </ModalFooter>
      </Modal>
    ),
    [...dependencies, returnValue, setReturnValue]
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
