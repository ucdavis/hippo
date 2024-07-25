import React from "react";
import { InputGroup, InputGroupProps, InputGroupText } from "reactstrap";

interface InputGroupWrapperProps extends InputGroupProps {
  children: React.ReactNode;
  prepend?: React.ReactNode;
  append?: React.ReactNode;
}

const InputGroupWrapper: React.FC<InputGroupWrapperProps> = ({
  children,
  prepend,
  append,
}) => {
  if (!prepend && !append) {
    return <>{children}</>;
  }

  return (
    <InputGroup>
      {!!prepend && <InputGroupText>{prepend}</InputGroupText>}
      {children}
      {!!append && <InputGroupText>{append}</InputGroupText>}
    </InputGroup>
  );
};

export default InputGroupWrapper;
