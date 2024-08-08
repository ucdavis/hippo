import React from "react";
import { Table, TableProps } from "reactstrap";

interface HipDumbTableProps extends TableProps {}

const HipDumbTable: React.FC<HipDumbTableProps> = ({ ...deferred }) => {
  return <Table {...deferred} striped={true} bordered={true}></Table>;
};

export default HipDumbTable;
