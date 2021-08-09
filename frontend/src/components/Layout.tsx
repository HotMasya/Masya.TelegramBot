import React, { PropsWithChildren, useState } from 'react';
import Sidebar, { SidebarProps } from './Sidebar';;
import ContentBox from './containers/ContentBox';
import Header from './Header';
import MiniProfile, { MiniProfileProps } from './MiniProfile';

import fakeAvatar from '../static/images/fake_avatar.jpg';
import { Button, Popover, Typography } from '@material-ui/core';

const Layout: React.FC = (props) => {
    const { children } = props;
    const [sidebarOpen, setSidebarOpen] = useState(false);
    const [anchorEl, setAnchorEl] = useState<Element>();
    const onPopoverClose = () => {
        setAnchorEl(undefined);
    }
    const onProfileClick = (event: React.MouseEvent) => {
        setAnchorEl(event.currentTarget);
    }
    const isPopoverOpen = Boolean(anchorEl);

    return (
        <>
            <Sidebar
                onOpen={() => setSidebarOpen(true)}
                onClose={() => setSidebarOpen(false)}
                onCloseClick={() => setSidebarOpen(false)}
                open={sidebarOpen}
            />
            <ContentBox>
                <Header onMenuClick={() => setSidebarOpen(state => !state)}>
                    <MiniProfile firstName="Masya" lastName="Poltorashka" avatar={fakeAvatar} onClick={onProfileClick} />
                    <Popover
                        onClose={onPopoverClose}
                        anchorEl={anchorEl}
                        open={isPopoverOpen}
                        anchorOrigin={{
                            vertical: 'bottom',
                            horizontal: 'right',
                        }}
                    >
                        <Button>
                            <Typography variant="h5" color="error">Log Out</Typography>
                        </Button>
                    </Popover>
                </Header>
                {children}
            </ContentBox>
        </>
    )
}

export default Layout;
