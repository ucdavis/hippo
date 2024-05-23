import React from "react";
import { Button, ButtonProps } from "reactstrap";

interface HipButtonProps extends ButtonProps {}

const HipButton: React.FC<HipButtonProps> = ({
  disabled = false,
  color = "primary",
  outline = false,
  children,
  ...props
}) => {
  return (
    <Button
      outline={!disabled} // for styling
      disabled={disabled}
      color={color}
      {...props}
    >
      {children}
    </Button>
  );
};

export default HipButton;
