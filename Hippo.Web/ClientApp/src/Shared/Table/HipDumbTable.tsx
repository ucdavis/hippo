import React from "react";
import { Table, TableProps } from "reactstrap";

interface HipDumbTableProps extends TableProps {
  children: React.ReactNode;
}

const HipDumbTable: React.FC<HipDumbTableProps> = ({
  children,
  ...deferred
}) => {
  return (
    <Table {...deferred} striped={true} bordered={true}>
      {children}
    </Table>
  );
};

export default HipDumbTable;
