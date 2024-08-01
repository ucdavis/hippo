import React from "react";
import { FormGroup } from "reactstrap";

interface HipFormGroupProps {
  wrap?: boolean;
  readOnly?: boolean;
  className?: string;
  children: React.ReactNode;
}

/**
 * HipFormGroup component that wraps reactstrap FormGroup with additional classNames.
 *
 * 'hip-form-group' is added to the FormGroup, 'no-wrap' is added if `wrap` is false,
 * 'read-only' is added if `readOnly` is true.
 */
export const HipFormGroup: React.FC<HipFormGroupProps> = ({
  wrap,
  readOnly,
  className,
  children,
  ...rest
}) => {
  return (
    <FormGroup
      className={`hip-form-group ${className ?? ""}${!wrap ? " no-wrap" : ""}${readOnly ? " read-only" : ""}`}
      {...rest}
    >
      {children}
    </FormGroup>
  );
};
