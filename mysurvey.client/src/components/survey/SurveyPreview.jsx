import React, { useState, useEffect } from 'react';
import { Card, Button, Space, message } from 'antd';
import { useNavigate, useParams } from 'react-router-dom';
import { surveyApi } from '../../services/api';
import QuestionList from './QuestionList';

const SurveyPreview = () => {
    const [survey, setSurvey] = useState(null);
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();
    const { id } = useParams();

    useEffect(() => {
        fetchSurvey();
    }, [id]);

    const fetchSurvey = async () => {
        try {
            setLoading(true);
            const response = await surveyApi.getSurvey(id);
            setSurvey(response.data);
        } catch (error) {
            message.error('获取问卷信息失败');
        } finally {
            setLoading(false);
        }
    };

    const handlePublish = async () => {
        try {
            await surveyApi.publishSurvey(id);
            message.success('发布成功');
            fetchSurvey();
        } catch (error) {
            message.error('发布失败');
        }
    };

    const handleEnd = async () => {
        try {
            await surveyApi.endSurvey(id);
            message.success('结束成功');
            fetchSurvey();
        } catch (error) {
            message.error('结束失败');
        }
    };

    if (loading || !survey) {
        return <div>加载中...</div>;
    }

    return (
        <div>
            <Card
                title={survey.title}
                extra={
                    <Space>
                        {survey.status === 0 && (
                            <Button type="primary" onClick={handlePublish}>
                                发布问卷
                            </Button>
                        )}
                        {survey.status === 1 && (
                            <Button danger onClick={handleEnd}>
                                结束问卷
                            </Button>
                        )}
                        <Button onClick={() => navigate(`/surveys/${id}/edit`)}>
                            编辑问卷
                        </Button>
                    </Space>
                }
            >
                <div style={{ marginBottom: 24 }}>
                    <h3>问卷描述</h3>
                    <p>{survey.description || '暂无描述'}</p>
                </div>

                <div style={{ marginBottom: 24 }}>
                    <h3>问卷时间</h3>
                    <p>开始时间：{new Date(survey.startTime).toLocaleString()}</p>
                    <p>结束时间：{new Date(survey.endTime).toLocaleString()}</p>
                </div>

                <div>
                    <h3>问题列表</h3>
                    <QuestionList
                        surveyId={id}
                        questions={survey.questions}
                        readOnly
                    />
                </div>
            </Card>
        </div>
    );
};

export default SurveyPreview; 