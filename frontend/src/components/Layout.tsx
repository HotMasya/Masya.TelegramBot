import React, { PropsWithChildren, useState } from 'react';
import Sidebar from './Sidebar';;
import ContentBox from './ContentBox';
import Header from './Header';

export interface LayoutProps { }

export default function Layout(props: PropsWithChildren<LayoutProps>) {
    const { children } = props;
    const [sidebarOpen, setSidebarOpen] = useState(false);

    return (
        <>
            <Sidebar
                onOpen={() => setSidebarOpen(true)}
                onClose={() => setSidebarOpen(false)}
                onCloseClick={() => setSidebarOpen(false)}
                open={sidebarOpen}
            />
            <ContentBox>
                <Header onMenuClick={() => setSidebarOpen(state => !state)} />
                {children}
            </ContentBox>
        </>
    )
}