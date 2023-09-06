import React from "react";
import DataTableBase, { TableProps } from "react-data-table-component";
import { InputGroup, Input, Button, Row, Col, Form } from "reactstrap";

interface FilterProps {
  filterText: string;
  onFilter: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onClear: () => void;
}

const Filter = ({ filterText, onFilter, onClear }: FilterProps) => (
  <Form size="sm">
    <Row>
      <Col sm={{ size: "auto" }}>
        <InputGroup>
          <Input
            id="search"
            type="text"
            placeholder="Search"
            aria-label="Search Input"
            value={filterText}
            onChange={onFilter}
          />
          <Button size="sm" onClick={onClear}>
            X
          </Button>
        </InputGroup>
      </Col>
    </Row>
  </Form>
);

export function DataTable<T>(props: TableProps<T>): JSX.Element {
  const [filterText, setFilterText] = React.useState("");
  const [resetPaginationToggle, setResetPaginationToggle] =
    React.useState(false);
  const filterTextLowerCase = filterText.toLowerCase();
  const filteredData = props.data.filter(
    (item) =>
      !filterText ||
      Object.values(item as object).some(
        (v) => v && v.toString().toLowerCase().includes(filterTextLowerCase)
      )
  );

  const filterSubHeader = React.useMemo(() => {
    const handleClear = () => {
      if (filterText) {
        setResetPaginationToggle(!resetPaginationToggle);
        setFilterText("");
      }
    };

    return (
      <Filter
        onFilter={(e) => setFilterText(e.target.value)}
        onClear={handleClear}
        filterText={filterText}
      />
    );
  }, [filterText, resetPaginationToggle]);

  return (
    <DataTableBase
      {...props}
      pagination
      paginationResetDefaultPage={resetPaginationToggle}
      subHeader
      subHeaderComponent={filterSubHeader}
      dense
      data={filteredData}
      paginationRowsPerPageOptions={[10, 25, 50, 100]}
      className="table"
    />
  );
}
