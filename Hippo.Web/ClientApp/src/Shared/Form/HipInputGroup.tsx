import React from "react";
import { InputGroup, InputGroupProps, InputGroupText } from "reactstrap";

interface InputGroupWrapperProps extends InputGroupProps {
  children: React.ReactNode;
  prepend?: React.ReactNode;
  append?: React.ReactNode;
  readOnly?: boolean;
}

/**
 * Wraps an input group with prepend and append elements.
 * Will return the children if readOnly or no prepend/append
 */
const HipInputGroup: React.FC<InputGroupWrapperProps> = ({
  children,
  prepend,
  append,
  readOnly,
}) => {
  if (readOnly || (!prepend && !append)) {
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

export default HipInputGroup;
