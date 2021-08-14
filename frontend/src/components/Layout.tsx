import React, { useState } from 'react';
import Sidebar from './Sidebar';
import ContentBox from './containers/ContentBox';
import Header from './Header';
import MiniProfile from './MiniProfile';

import fakeAvatar from '../static/images/fake_avatar.jpg';
import { Button, Popover, Typography } from '@material-ui/core';
import RootState from '../store/reducers';
import { useSelector } from 'react-redux';
import { Redirect } from 'react-router';
import { endpoints } from '../routing/endpoints';

const Layout: React.FC = (props) => {
  const { children } = props;
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [anchorEl, setAnchorEl] = useState<Element>();
  const onPopoverClose = () => {
    setAnchorEl(undefined);
  };
  const onProfileClick = (event: React.MouseEvent) => {
    setAnchorEl(event.currentTarget);
  };
  const isPopoverOpen = Boolean(anchorEl);
  const { user } = useSelector((state: RootState) => state.account);
  if (!user) {
    return <Redirect to={endpoints.auth} />;
  }

  return (
    <>
      <Sidebar
        onOpen={() => setSidebarOpen(true)}
        onClose={() => setSidebarOpen(false)}
        onCloseClick={() => setSidebarOpen(false)}
        open={sidebarOpen}
      />
      <ContentBox>
        <Header onMenuClick={() => setSidebarOpen((state) => !state)}>
          <MiniProfile
            firstName={user.firstName}
            lastName={user.lastName}
            avatar={fakeAvatar}
            onClick={onProfileClick}
          />
          <Popover
            onClose={onPopoverClose}
            anchorEl={anchorEl}
            open={isPopoverOpen}
            anchorOrigin={{
              vertical: 'bottom',
              horizontal: 'right',
            }}>
            <Button>
              <Typography variant="h5" color="error">
                Log Out
              </Typography>
            </Button>
          </Popover>
        </Header>
        {children}
      </ContentBox>
    </>
  );
};

export default Layout;
