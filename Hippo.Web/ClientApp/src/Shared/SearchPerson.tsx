import React, { useState } from "react";

import {
  ClearButton,
  AsyncTypeahead,
  Highlighter,
} from "react-bootstrap-typeahead";
import { useIsMounted } from "../Shared/UseIsMounted";
import { authenticatedFetch } from "../util/api";

import { User } from "../types";

type TypeaheadProps = React.ComponentProps<typeof AsyncTypeahead>;

interface Props extends Pick<TypeaheadProps, "onBlur"> {
  user?: User;
  onChange: (user: User | undefined) => void;
}

export const SearchPerson = (props: Props) => {
  const [isSearchLoading, setIsSearchLoading] = useState<boolean>(false);
  const [users, setUsers] = useState<User[]>([]);

  const getIsMounted = useIsMounted();
  const onSearch = async (query: string) => {
    setIsSearchLoading(true);

    const response = await authenticatedFetch(
      `/api/people/search?query=${query}`,
    );

    if (response.ok) {
      if (response.status === 204) {
        getIsMounted() && setUsers([]); // no content means no match
      } else {
        const user: User = await response.json();

        getIsMounted() && setUsers([user]);
      }
    }
    getIsMounted() && setIsSearchLoading(false);
  };

  const onSelect = (selected: User[]) => {
    if (selected && selected.length === 1) {
      // found our match
      props.onChange(selected[0]);
    } else {
      props.onChange(undefined);
    }
  };

  return (
    <AsyncTypeahead
      id="searchPeople" // for accessibility
      isLoading={isSearchLoading}
      minLength={3}
      defaultSelected={props.user ? [props.user] : []}
      placeholder="Search for person by email or kerberos"
      labelKey={(option: User) => `${option.name} (${option.email})`}
      filterBy={() => true} // don't filter on top of our search
      renderMenuItemChildren={(option: User, propsData, index) => (
        <div>
          <div>
            <Highlighter key="name" search={propsData.text || ""}>
              {option.name}
            </Highlighter>
          </div>
          <div>
            <Highlighter key="email" search={propsData.text || ""}>
              {option.email}
            </Highlighter>
          </div>
        </div>
      )}
      onSearch={onSearch}
      onChange={onSelect}
      options={users}
      onBlur={props.onBlur}
      disabled={props.user !== undefined}
    >
      {({ onClear, selected }: { onClear: any; selected: any }) => (
        <div className="searchPersonsClearButton rbt-aux">
          {!!selected.length && <ClearButton onClick={onClear} />}
        </div>
      )}
    </AsyncTypeahead>
  );
};
